using System;
using System.Collections.Generic;
using System.Reflection;
using UIWidgetsApp.Component.PullToRefresh.Indicator;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.scheduler;
using Unity.UIWidgets.widgets;
using UnityEngine;

namespace UIWidgetsApp.Component.PullToRefresh
{
    public static class RefreshStatus
    {
        public const int idle = 0;
        public const int canRefresh = 1;
        public const int refreshing = 2;
        public const int completed = 3;
        public const int failed = 4;
        public const int noMore = 5;
    }

    public class SmartRefresher : StatefulWidget
    {
        public SmartRefresher(
            ScrollView child,
            float initialOffset = 0f,
            IndicatorBuilder headerBuilder = null,
            IndicatorBuilder footerBuilder = null,
            Config headerConfig = null,
            Config footerConfig = null,
            bool enablePullUp = DefaultConstants.default_enablePullUp,
            bool enablePullDown = DefaultConstants.default_enablePullDown,
            bool enableOverScroll = DefaultConstants.default_enableOverScroll,
            bool reverse = false,
            OnRefresh onRefresh = null,
            OnOffsetChange onOffsetChange = null,
            RefreshController controller = null,
            NotificationListenerCallback<ScrollNotification> onNotification = null,
            Key key = null
        ) : base(key: key)
        {
            this.child = child;
            this.initialOffset = initialOffset;
            this.headerBuilder = headerBuilder ?? ((context, mode) => new ClassicIndicator(mode: mode));
            this.footerBuilder = footerBuilder ?? ((context, mode) => new ClassicIndicator(mode: mode));
            this.headerConfig = headerConfig ?? new RefreshConfig();
            this.footerConfig = footerConfig ?? new LoadConfig(triggerDistance: 0);
            this.enablePullUp = enablePullUp;
            this.enablePullDown = enablePullDown;
            this.enableOverScroll = enableOverScroll;
            this.onRefresh = onRefresh;
            this.onOffsetChange = onOffsetChange;
            this.controller = controller ?? new RefreshController();
            this.onNotification = onNotification;
            this.reverse = reverse;
        }

        public readonly ScrollView child;

        public readonly IndicatorBuilder headerBuilder;

        public readonly IndicatorBuilder footerBuilder;

        // configure your header and footer
        public readonly Config headerConfig, footerConfig;

        // This bool will affect whether or not to have the function of drop-up load.
        public readonly bool enablePullUp;

        //This bool will affect whether or not to have the function of drop-down refresh.
        public readonly bool enablePullDown;

        // if open OverScroll if you use RefreshIndicator and LoadFooter
        public readonly bool enableOverScroll;

        // upper and downer callback when you drag out of the distance
        public readonly OnRefresh onRefresh;

        // This method will callback when the indicator changes from edge to edge.
        public readonly OnOffsetChange onOffsetChange;

        //controller inner state
        public readonly RefreshController controller;

        public readonly float initialOffset;

        public readonly NotificationListenerCallback<ScrollNotification> onNotification;

        public readonly bool reverse;

        public override State createState()
        {
            return new _SmartRefresherState();
        }
    }

    public class _SmartRefresherState : State<SmartRefresher>
    {
        private ScrollController _scrollController;
        private readonly GlobalKey _headerKey = GlobalKey.key();
        private readonly GlobalKey _footerKey = GlobalKey.key();
        private float _headerHeight = DefaultConstants.default_VisibleRange;
        private float _footerHeight = DefaultConstants.default_VisibleRange;
        private readonly ValueNotifier<float> offsetLis = new ValueNotifier<float>(0.0f);
        private readonly ValueNotifier<int> topModeLis = new ValueNotifier<int>(0);
        private readonly ValueNotifier<int> bottomModeLis = new ValueNotifier<int>(0);

        private bool _handleScrollStart(ScrollStartNotification notification)
        {
            // This is used to interrupt useless callback when the pull up load rolls back.
            if (notification.metrics.outOfRange()) return false;

            var topWrap = _headerKey.currentState as GestureProcessor;
            var bottomWrap = _footerKey.currentState as GestureProcessor;
            if (widget.enablePullUp) bottomWrap.onDragStart(notification: notification);

            if (widget.enablePullDown) topWrap.onDragStart(notification: notification);

            return false;
        }

        private bool _handleScrollMoving(ScrollUpdateNotification notification)
        {
            if (_measure(notification: notification) != -1.0) offsetLis.value = _measure(notification: notification);

            var topWrap = _headerKey.currentState as GestureProcessor;
            var bottomWrap = _footerKey.currentState as GestureProcessor;
            if (widget.enablePullUp) bottomWrap.onDragMove(notification: notification);

            if (widget.enablePullDown) topWrap.onDragMove(notification: notification);

            return false;
        }

        private bool _handleScrollEnd(ScrollNotification notification)
        {
            var topWrap = _headerKey.currentState as GestureProcessor;
            var bottomWrap = _footerKey.currentState as GestureProcessor;
            if (widget.enablePullUp) bottomWrap.onDragEnd(notification: notification);

            if (widget.enablePullDown) topWrap.onDragEnd(notification: notification);

            return false;
        }

        private bool _dispatchScrollEvent(ScrollNotification notification)
        {
            widget.onNotification?.Invoke(notification: notification);

            var axisDirection = notification.metrics.axisDirection;
            var scrollDirection = widget.child.scrollDirection;
            if ((axisDirection == AxisDirection.left || axisDirection == AxisDirection.right) &&
                scrollDirection == Axis.vertical)
                return false;

            if ((axisDirection == AxisDirection.up || axisDirection == AxisDirection.down) &&
                scrollDirection == Axis.horizontal)
                return false;

            // when is scroll in the ScrollInside,nothing to do
            if (!_isPullUp(notification: notification) && !_isPullDown(notification: notification)) return false;

            if (notification is ScrollStartNotification startNotification)
                return _handleScrollStart(notification: startNotification);

            if (notification is ScrollUpdateNotification updateNotification)
            {
                //if dragDetails is null,This represents the user's finger out of the screen
                if (updateNotification.dragDetails == null) return _handleScrollEnd(notification: notification);

                if (updateNotification.dragDetails != null)
                    return _handleScrollMoving(notification: updateNotification);
            }

            if (notification is ScrollEndNotification) _handleScrollEnd(notification: notification);

            return false;
        }

        private static bool _isPullUp(ScrollNotification notification)
        {
            return notification.metrics.pixels < 0;
        }

        private static bool _isPullDown(ScrollNotification notification)
        {
            return notification.metrics.pixels > 0;
        }

        private float _measure(ScrollNotification notification)
        {
            if (notification.metrics.minScrollExtent - notification.metrics.pixels >
                0)
                return (notification.metrics.minScrollExtent -
                        notification.metrics.pixels) / widget.headerConfig.triggerDistance;

            if (notification.metrics.pixels -
                notification.metrics.maxScrollExtent >
                0)
                return (notification.metrics.pixels -
                        notification.metrics.maxScrollExtent) / widget.footerConfig.triggerDistance;

            return -1.0f;
        }

        private void _init()
        {
            _scrollController = widget.controller.scrollController?? new ScrollController(initialScrollOffset: widget.initialOffset);
            SchedulerBinding.instance.addPostFrameCallback(duration => { _onAfterBuild(); });
            _scrollController.addListener(listener: _handleOffsetCallback);
            widget.controller._headerMode = topModeLis;
            widget.controller._footerMode = bottomModeLis;
        }

        private void _handleOffsetCallback()
        {
            var overScrollPastStart = Mathf.Max(_scrollController.position.minScrollExtent -
                                                _scrollController.position.pixels +
                                                (widget.headerConfig is RefreshConfig &&
                                                 (topModeLis.value == RefreshStatus.refreshing ||
                                                  topModeLis.value == RefreshStatus.completed ||
                                                  topModeLis.value == RefreshStatus.failed)
                                                    ? (widget.headerConfig as RefreshConfig).visibleRange
                                                    : 0.0f),
                0.0f);
            var overScrollPastEnd = Mathf.Max(_scrollController.position.pixels -
                                              _scrollController.position.maxScrollExtent +
                                              (widget.footerConfig is RefreshConfig &&
                                               (bottomModeLis.value == RefreshStatus.refreshing ||
                                                bottomModeLis.value == RefreshStatus.completed ||
                                                bottomModeLis.value == RefreshStatus.failed)
                                                  ? (widget.footerConfig as RefreshConfig).visibleRange
                                                  : 0.0f),
                0.0f);
            if (overScrollPastStart > overScrollPastEnd)
            {
                if (widget.headerConfig is RefreshConfig)
                {
                    if (widget.onOffsetChange != null) widget.onOffsetChange(true, offset: overScrollPastStart);
                }
                else
                {
                    if (widget.onOffsetChange != null) widget.onOffsetChange(true, offset: overScrollPastStart);
                }
            }
            else if (overScrollPastEnd > 0)
            {
                if (widget.footerConfig is RefreshConfig)
                {
                    if (widget.onOffsetChange != null) widget.onOffsetChange(false, offset: overScrollPastEnd);
                }
                else
                {
                    if (widget.onOffsetChange != null) widget.onOffsetChange(false, offset: overScrollPastEnd);
                }
            }
        }

        private void _didChangeMode(bool up, ValueNotifier<int> mode)
        {
            switch (mode.value)
            {
                case RefreshStatus.refreshing:
                    if (widget.onRefresh != null) widget.onRefresh(up: up);

                    if (up && widget.headerConfig is RefreshConfig config)
                        _scrollController
                            .jumpTo(_scrollController.offset + config.visibleRange);

                    break;
            }
        }

        private void _onAfterBuild()
        {
            if (widget.headerConfig is LoadConfig loadConfig)
                if (loadConfig.bottomWhenBuild)
                    _scrollController.jumpTo(-(_scrollController.position.pixels -
                                               _scrollController.position.maxScrollExtent));

            topModeLis.addListener(() => { _didChangeMode(true, mode: topModeLis); });
            bottomModeLis.addListener(() => { _didChangeMode(false, mode: bottomModeLis); });
            setState(() =>
            {
                if (widget.enablePullDown && _headerKey.currentContext != null)
                    _headerHeight = _headerKey.currentContext.size.height;

                if (widget.enablePullUp && _footerKey.currentContext != null)
                    _footerHeight = _footerKey.currentContext.size.height;
            });
        }

        private Widget _buildWrapperByConfig(Config config, bool up)
        {
            if (config is LoadConfig loadConfig)
                return new LoadWrapper(
                    key: up ? _headerKey : _footerKey,
                    modeListener: up ? topModeLis : bottomModeLis,
                    up: up,
                    autoLoad: loadConfig.autoLoad,
                    triggerDistance: loadConfig.triggerDistance,
                    builder: up ? widget.headerBuilder : widget.footerBuilder
                );

            if (config is RefreshConfig refreshConfig)
                return new RefreshWrapper(
                    key: up ? _headerKey : _footerKey,
                    modeListener: up ? topModeLis : bottomModeLis,
                    up: up,
                    onOffsetChange: (offsetUp, offset) =>
                    {
                        if (widget.onOffsetChange != null)
                            widget.onOffsetChange(
                                up: offsetUp,
                                offsetUp
                                    ? -_scrollController.offset + offset
                                    : _scrollController.position.pixels -
                                      _scrollController.position.maxScrollExtent +
                                      offset);
                    },
                    completeDuration: refreshConfig.completeDuration,
                    triggerDistance: refreshConfig.triggerDistance,
                    visibleRange: refreshConfig.visibleRange,
                    builder: up ? widget.headerBuilder : widget.footerBuilder
                );

            return new Container();
        }

        public override void dispose()
        {
            _scrollController.removeListener(listener: _handleOffsetCallback);
            _scrollController.dispose();
            base.dispose();
        }

        public override void initState()
        {
            base.initState();
            _init();
        }

        public override Widget build(BuildContext context)
        {
            var type = typeof(ScrollView);
            var method = type.GetMethod("buildSlivers", BindingFlags.NonPublic | BindingFlags.Instance);
            var objs = new object[1];
            objs[0] = context;
            var slivers = (List<Widget>)method.Invoke(obj: widget.child, parameters: objs);
            slivers.Add(new SliverToBoxAdapter(
                child: widget.footerBuilder != null && widget.enablePullUp
                    ? _buildWrapperByConfig(config: widget.footerConfig, false)
                    : new Container()
            ));
            slivers.Insert(
                0,
                new SliverToBoxAdapter(
                    child: widget.headerBuilder != null && widget.enablePullDown
                        ? _buildWrapperByConfig(config: widget.headerConfig, true)
                        : new Container()));
            return new LayoutBuilder(builder: (cxt, cons) =>
            {
                return new Stack(
                    children: new List<Widget>
                    {
                        new Positioned(
                            top: !widget.enablePullDown || widget.headerConfig is LoadConfig
                                ? 0.0f
                                : -_headerHeight,
                            bottom: !widget.enablePullUp || widget.footerConfig is LoadConfig
                                ? 0.0f
                                : -_footerHeight,
                            left: 0.0f,
                            right: 0.0f,
                            child: new NotificationListener<ScrollNotification>(
                                child: new CustomScrollView(
                                    reverse: widget.reverse,
                                    physics: widget.enablePullUp
                                        ? (ScrollPhysics) new EnablePullUpRefreshScrollPhysics(
                                            enableOverScroll: widget.enableOverScroll)
                                        : new DisablePullUpRefreshScrollPhysics(
                                            enableOverScroll: widget.enableOverScroll),
                                    controller: _scrollController,
                                    slivers: slivers
                                ),
                                onNotification: _dispatchScrollEvent
                            )
                        )
                    }
                );
            });
        }
    }


    public abstract class IndicatorWidget : StatefulWidget
    {
        public readonly int mode;

        protected IndicatorWidget(
            int mode,
            Key key = null
        ) : base(key: key)
        {
            this.mode = mode;
        }
    }


    public class RefreshController
    {
        public ValueNotifier<int> _headerMode = new ValueNotifier<int>(0);
        public ValueNotifier<int> _footerMode = new ValueNotifier<int>(0);
        public ScrollController scrollController;

        public RefreshController(ScrollController scrollController = null)
        {
            this.scrollController = scrollController;
        }
        
        public void requestRefresh(bool up)
        {
            if (up)
            {
                if (_headerMode.value == RefreshStatus.idle) _headerMode.value = RefreshStatus.refreshing;
            }
            else
            {
                if (_footerMode.value == RefreshStatus.idle) _footerMode.value = RefreshStatus.refreshing;
            }
        }

        public void scrollTo(float offset)
        {
            scrollController?.jumpTo(value: offset);
        }

        public void animateTo(float to, TimeSpan duration, Curve curve)
        {
            scrollController?.animateTo(to: to, duration: duration, curve: curve);
        }

        public float offset => scrollController.offset;

        public void sendBack(bool up, int mode)
        {
            SchedulerBinding.instance.addPostFrameCallback(_ =>
            {
                if (up)
                    _headerMode.value = mode;
                else
                    _footerMode.value = mode;
            });
        }

        public bool isRefresh(bool up)
        {
            if (up)
                return _headerMode.value == RefreshStatus.refreshing;
            else
                return _footerMode.value == RefreshStatus.refreshing;
        }

        public int headerMode => _headerMode.value;

        public int footerMode => _footerMode.value;
    }
}