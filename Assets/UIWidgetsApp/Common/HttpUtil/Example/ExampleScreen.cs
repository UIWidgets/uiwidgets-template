using System.Collections.Generic;
using uiwidgets;
using Unity.UIWidgets.material;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.widgets;

namespace UIWidgetsApp.Common.HttpUtil.Example
{
    public class ExampleScreen : StatefulWidget
    {
        public override State createState()
        {
            return new ExampleScreenState();
        }
    }

    internal class ExampleScreenState : State<ExampleScreen>
    {
        private ExampleApi.StatusResponse response;

        private Widget BuildContentWidget()
        {
            if (response != null)
            {
                return new Text(
                    response.status.description
                );
            }

            return new Container();
        }
        
        public override Widget build(BuildContext context)
        {
            return new Scaffold(
                body: new Center(
                    child: new Column(
                        mainAxisAlignment: MainAxisAlignment.center,
                        children: new List<Widget>
                        {
                            new FlatButton(
                                onPressed: () => ExampleApi.GetGitHubStatus().then(value =>
                                {
                                    if (!(value is ExampleApi.StatusResponse statusResponse)) {
                                        return;
                                    }
                                    response = statusResponse;
                                    setState(() => {});
                                }),
                                color: Colors.blueAccent,
                                child: new Text(
                                    "Fetch GitHub Status",
                                    style: new TextStyle(color: Colors.white, fontSize: 12)
                                )
                            ),
                            BuildContentWidget()
                        }
                    )
                )
            );
        }
    }
}