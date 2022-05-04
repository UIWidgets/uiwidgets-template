using System.Collections.Generic;
using uiwidgets;
using UIWidgetsApp.Model;
using UIWidgetsApp.Redux.Action;
using UIWidgetsApp.Redux.State;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.material;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.Redux;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.widgets;

namespace UIWidgetsApp.Screen
{
    public class PageScreenConnector : StatelessWidget
    {
        public PageScreenConnector(
            string pageName,
            Key key = null
        ) : base(key)
        {
            this.pageName = pageName;
        }

        private readonly string pageName;

        public override Widget build(BuildContext context)
        {
            return new StoreConnector<AppState, PageViewModel>(
                converter: state => new PageViewModel
                {
                    pageName = pageName,
                    count = state.testState.count
                },
                builder: (context1, viewModel, dispatcher) =>
                {
                    var actionModel = new BaseActionModel
                    {
                        add = added => dispatcher.dispatch(new AddCountAction { added = added })
                    };
                    return new PageScreen(viewModel: viewModel, actionModel: actionModel);
                }
            );
        }
    }

    public class PageScreen : StatefulWidget
    {
        public PageScreen(
            Key key = null,
            PageViewModel viewModel = null,
            BaseActionModel actionModel = null
        ) : base(key)
        {
            this.viewModel = viewModel;
            this.actionModel = actionModel;
        }

        public readonly PageViewModel viewModel;
        public readonly BaseActionModel actionModel;

        public override State createState()
        {
            return new PageScreenState();
        }
    }

    internal class PageScreenState : State<PageScreen>
    {
        private Widget BuildContent()
        {
            return new Container(
                child: new Center(
                    child: new Column(
                        mainAxisAlignment: MainAxisAlignment.spaceBetween,
                        mainAxisSize: MainAxisSize.max,
                        children: new List<Widget>
                        {
                            new Text("COUNT: " + widget.viewModel.count, style: new TextStyle(fontSize: 40)),
                            new Row(
                                mainAxisAlignment: MainAxisAlignment.spaceEvenly,
                                mainAxisSize: MainAxisSize.max,
                                children: new List<Widget>
                                {
                                    new ButtonTheme(
                                        minWidth: 100,
                                        child: new FlatButton(
                                            onPressed: () => widget.actionModel.add(1),
                                            color: Colors.blueAccent,
                                            child: new Text(
                                                "+ 1",
                                                style: new TextStyle(color: Colors.white, fontSize: 12)
                                            )
                                        )
                                    ),
                                    new ButtonTheme(
                                        minWidth: 100,
                                        child: new FlatButton(
                                            onPressed: () => widget.actionModel.add(5),
                                            color: Colors.blueAccent,
                                            child: new Text(
                                                "+ 5",
                                                style: new TextStyle(color: Colors.white, fontSize: 12)
                                            )
                                        )
                                    ),
                                    new ButtonTheme(
                                        minWidth: 100,
                                        child: new FlatButton(
                                            onPressed: () => widget.actionModel.add(10),
                                            color: Colors.blueAccent,
                                            child: new Text(
                                                "+ 10",
                                                style: new TextStyle(color: Colors.white, fontSize: 12)
                                            )
                                        )
                                    )
                                }
                            )
                        }
                    )
                )
            );
        }

        public override Widget build(BuildContext buildContext)
        {
            return new Scaffold(
                appBar: new AppBar(
                    title: new Text(widget.viewModel.pageName.ToUpper() + " SCREEN",
                        style: new TextStyle(fontSize: 24)
                    )
                ),
                body: new Container(
                    color: Colors.white,
                    child: BuildContent()
                )
            );
        }
    }
}