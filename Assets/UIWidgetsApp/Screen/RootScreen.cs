using System.Collections.Generic;
using uiwidgets;
using UIWidgetsApp.Main;
using UIWidgetsApp.Model;
using UIWidgetsApp.Redux.State;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.material;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.Redux;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.widgets;

namespace UIWidgetsApp.Screen
{
    public class RootScreenConnector : StatelessWidget
    {
        public RootScreenConnector(
            Key key = null
        ) : base(key)
        {
        }

        public override Widget build(BuildContext context)
        {
            return new StoreConnector<AppState, BaseViewModel>(
                converter: state => new BaseViewModel(),
                builder: (context1, viewModel, dispatcher) =>
                {
                    var actionModel = new BaseActionModel();
                    return new RootScreen(viewModel: viewModel, actionModel: actionModel);
                }
            );
        }
    }

    public class RootScreen : StatefulWidget
    {
        public RootScreen(
            Key key = null,
            BaseViewModel viewModel = null,
            BaseActionModel actionModel = null
        ) : base(key)
        {
            this.viewModel = viewModel;
            this.actionModel = actionModel;
        }

        public readonly BaseViewModel viewModel;
        public readonly BaseActionModel actionModel;

        public override State createState()
        {
            return new RootScreenState();
        }
    }

    internal class RootScreenState : State<RootScreen>
    {
        private Widget BuildContent()
        {
            return new Container(
                child: new Center(
                    child: new Column(
                        mainAxisAlignment: MainAxisAlignment.spaceEvenly,
                        mainAxisSize: MainAxisSize.max,
                        children: new List<Widget>
                        {
                            new FlatButton(
                                onPressed: () => Navigator.pushNamed(context, NavigatorRoutes.Refresh),
                                color: Colors.blueAccent,
                                child: new Text(
                                    "Jump to RefreshListPage",
                                    style: new TextStyle(color: Colors.white, fontSize: 12)
                                )
                            ),
                            new FlatButton(
                                onPressed: () => Navigator.pushNamed(context, NavigatorRoutes.Page,
                                    new PageScreenArguments { pageName = "page1" }),
                                color: Colors.blueAccent,
                                child: new Text(
                                    "Jump to Page1",
                                    style: new TextStyle(color: Colors.white, fontSize: 12)
                                )
                            ),
                            new FlatButton(
                                onPressed: () => Navigator.pushNamed(context, NavigatorRoutes.Page,
                                    new PageScreenArguments { pageName = "page2" }), color: Colors.blueAccent,
                                child: new Text(
                                    "Jump to Page2",
                                    style: new TextStyle(color: Colors.white, fontSize: 12)
                                )
                            )
                        }
                    )
                )
            );
        }

        public override Widget build(BuildContext buildContext)
        {
            return new Scaffold(
                appBar: new AppBar(title: new Text("ROOT SCREEN", style: new TextStyle(fontSize: 24))),
                body: new Container(
                    color: Colors.white,
                    child: BuildContent()
                )
            );
        }
    }
}