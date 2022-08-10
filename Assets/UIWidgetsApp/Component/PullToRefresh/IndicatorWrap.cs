using System;
using System.Collections.Generic;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.async;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.scheduler;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;

namespace UIWidgetsApp.Component.PullToRefresh
{
    public abstract class Wrapper : StatefulWidget
    {
        protected Wrapper(
            ValueNotifier<int> modeListener,
            bool up,
            IndicatorBuilder builder = null,
            float triggerDistance = 0,
            Key key = null
        ) : base(key: key)
        {
            this.modeListener = modeListener;
            this.up = up;
            this.builder = builder;
            this.triggerDistance = triggerDistance;
        }

        public readonly ValueNotifier<int> modeListener;

        public readonly IndicatorBuilder builder;

        public readonly bool up;

        public readonly float triggerDistance;

        public bool _isRefreshing => mode == RefreshStatus.refreshing;

        public bool _isComplete =>
            mode != RefreshStatus.idle && mode != RefreshStatus.refreshing &&
            mode != RefreshStatus.canRefresh;

        public int mode
        {
            get => modeListener.value;

            set => modeListener.value = value;
        }

        public bool _isScrollToOutSide(ScrollNotification notification)
        {
            if (up)
            {
                if (notification.metrics.minScrollExtent - notification.metrics.pixels >
                    0)
                    return true;
            }
            else
            {
                if (notification.metrics.pixels - notification.metrics.maxScrollExtent >
                    0)
                    return true;
            }

            return false;
        }
    }

    public class RefreshWrapper : Wrapper
    {
        public RefreshWrapper(
            bool up,
            ValueNotifier<int> modeListener = null,
            int completeDuration = DefaultConstants.default_completeDuration,
            OnOffsetChange onOffsetChange = null,
            float visibleRange = DefaultConstants.default_VisibleRange,
            IndicatorBuilder builder = null,
            float triggerDistance = 0,
            Key key = null
        ) : base(modeListener: modeListener, up: up, builder: builder, triggerDistance: triggerDistance, key: key)
        {
            this.completeDuration = completeDuration;
            this.onOffsetChange = onOffsetChange;
            this.visibleRange = visibleRange;
        }

        public readonly int completeDuration;

        public readonly OnOffsetChange onOffsetChange;

        public readonly float visibleRange;

        public override State createState()
        {
            return new _RefreshWrapperState();
        }
    }

    public class _RefreshWrapperState : State<RefreshWrapper>, TickerProvider, GestureProcessor
    {
        private AnimationController _sizeController;

        public override void initState()
        {
            base.initState();
            _sizeController = new AnimationController(
                vsync: this,
                lowerBound: DefaultConstants.minSpace,
                duration: TimeSpan.FromMilliseconds(value: DefaultConstants.spaceAnimateMill));
            _sizeController.addListener(listener: _handleOffsetCallBack);
            widget.modeListener.addListener(listener: _handleModeChange);
        }

        public override Widget build(BuildContext context)
        {
            if (widget.up)
                return new Column(
                    children: new List<Widget>
                    {
                        new SizeTransition(
                            sizeFactor: _sizeController,
                            child: new Container(height: widget.visibleRange)
                        ),
                        widget.builder(context: context, mode: widget.mode)
                    }
                );

            return new Column(
                children: new List<Widget>
                {
                    widget.builder(context: context, mode: widget.mode),
                    new SizeTransition(
                        sizeFactor: _sizeController,
                        child: new Container(height: widget.visibleRange)
                    )
                }
            );
        }

        private void _dismiss()
        {
            _sizeController.animateTo(target: DefaultConstants.minSpace)
                .then(_ => { widget.mode = RefreshStatus.idle; });
        }

        public int mode => widget.modeListener.value;

        private float _measure(ScrollNotification notification)
        {
            if (widget.up)
                return (notification.metrics.minScrollExtent -
                        notification.metrics.pixels) / widget.triggerDistance;

            return (notification.metrics.pixels -
                    notification.metrics.maxScrollExtent) / widget.triggerDistance;
        }

        public Ticker createTicker(TickerCallback onTick)
        {
            return new Ticker(onTick: onTick, $"created by {this}");
        }

        public void onDragStart(ScrollStartNotification notification)
        {
//            throw new NotImplementedException();
        }

        public void onDragMove(ScrollUpdateNotification notification)
        {
            if (!widget._isScrollToOutSide(notification: notification)) return;

            if (widget._isComplete || widget._isRefreshing) return;

            var offset = _measure(notification: notification);
            if (offset >= 1.0)
                widget.mode = RefreshStatus.canRefresh;
            else
                widget.mode = RefreshStatus.idle;
        }

        public void onDragEnd(ScrollNotification notification)
        {
            if (!widget._isScrollToOutSide(notification: notification)) return;

            if (widget._isComplete || widget._isRefreshing) return;

            var reachMax = _measure(notification: notification) >= 1.0;
            if (!reachMax)
                _sizeController.animateTo(0.0f);
            else
                widget.mode = RefreshStatus.refreshing;
        }

        private void _handleOffsetCallBack()
        {
            widget.onOffsetChange?.Invoke(
                up: widget.up, _sizeController.value * widget.visibleRange);
        }

        private void _handleModeChange()
        {
            switch (mode)
            {
                case RefreshStatus.refreshing:
                    _sizeController.setValue(1.0f);
                    break;
                case RefreshStatus.completed:
                    Future.delayed(
                        TimeSpan.FromMilliseconds(value: widget.completeDuration),
                        () =>
                        {
                            _dismiss();
                            return default;
                        }
                    );
                    break;
                case RefreshStatus.failed:
                    Future.delayed(
                        TimeSpan.FromMilliseconds(value: widget.completeDuration),
                        () =>
                        {
                            _dismiss();
                            return default;
                        }
                    ).then(_ => { widget.mode = RefreshStatus.idle; });
                    break;
            }

            setState(() => { });
        }

        public override void dispose()
        {
            widget.modeListener.removeListener(listener: _handleModeChange);
            _sizeController.removeListener(listener: _handleOffsetCallBack);
            base.dispose();
        }
    }


    public class LoadWrapper : Wrapper
    {
        public LoadWrapper(
            bool up,
            ValueNotifier<int> modeListener = null,
            bool autoLoad = true,
            IndicatorBuilder builder = null,
            float triggerDistance = 0,
            Key key = null
        ) : base(modeListener: modeListener, up: up, builder: builder, triggerDistance: triggerDistance, key: key)
        {
            this.autoLoad = autoLoad;
        }

        public readonly bool autoLoad;

        public override State createState()
        {
            return new _LoadWrapperState();
        }
    }

    public class _LoadWrapperState : State<LoadWrapper>, GestureProcessor
    {
        private VoidCallback _updateListener;


        public override void initState()
        {
            base.initState();
            _updateListener = () => { setState(() => { }); };
            widget.modeListener.addListener(listener: _updateListener);
        }

        public override Widget build(BuildContext context)
        {
            return widget.builder(context: context, mode: widget.mode);
        }


        public void onDragStart(ScrollStartNotification notification)
        {
//            throw new NotImplementedException();
        }

        public void onDragMove(ScrollUpdateNotification notification)
        {
            if (widget._isRefreshing || widget._isComplete) return;

            if (widget.autoLoad)
            {
                if (widget.up &&
                    notification.metrics.extentBefore() <= widget.triggerDistance)
                    widget.mode = RefreshStatus.refreshing;

                if (!widget.up &&
                    notification.metrics.extentAfter() <= widget.triggerDistance)
                    widget.mode = RefreshStatus.refreshing;
            }
        }

        public void onDragEnd(ScrollNotification notification)
        {
            if (widget._isRefreshing || widget._isComplete) return;

            if (widget.autoLoad)
            {
                if (widget.up &&
                    notification.metrics.extentBefore() <= widget.triggerDistance)
                    widget.mode = RefreshStatus.refreshing;

                if (!widget.up &&
                    notification.metrics.extentAfter() <= widget.triggerDistance)
                    widget.mode = RefreshStatus.refreshing;
            }
        }


        public override void dispose()
        {
            widget.modeListener.removeListener(listener: _updateListener);
            base.dispose();
        }
    }


    public interface GestureProcessor
    {
        void onDragStart(ScrollStartNotification notification);

        void onDragMove(ScrollUpdateNotification notification);

        void onDragEnd(ScrollNotification notification);
    }
}