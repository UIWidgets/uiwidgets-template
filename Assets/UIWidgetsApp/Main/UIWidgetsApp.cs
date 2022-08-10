using System.Collections.Generic;
using uiwidgets;
using UIWidgetsApp.Model;
using UIWidgetsApp.Redux;
using UIWidgetsApp.Redux.State;
using UIWidgetsApp.Screen;
using Unity.UIWidgets.material;
using Unity.UIWidgets.Redux;
using Unity.UIWidgets.widgets;

namespace UIWidgetsApp.Main
{
    public class UIWidgetsApp : StatelessWidget
    {
        public static RouteObserve<PageRoute> routeObserver;

        public override Widget build(BuildContext context)
        {
            routeObserver = new RouteObserve<PageRoute>();
            return new StoreProvider<AppState>(
                StoreProvider.Store,
                new MaterialApp(
                    color: Colors.white,
                    initialRoute: NavigatorRoutes.Root,
                    onGenerateRoute: OnGenerateRoute,
                    onUnknownRoute: OnUnknownRoute,
                    navigatorObservers: new List<NavigatorObserver>
                    {
                        routeObserver
                    }
                )
            );
        }

        private static Route OnGenerateRoute(RouteSettings settings)
        {
            RoutePageBuilder builder;
            switch (settings.name)
            {
                case NavigatorRoutes.Root:
                    builder = (context, animation, secondaryAnimation) => new RootScreenConnector();
                    break;
                case NavigatorRoutes.Page:
                {
                    var arg = settings.arguments as PageScreenArguments;
                    builder = (context, animation, secondaryAnimation) => new PageScreenConnector(arg.pageName);
                    break;
                }
                case NavigatorRoutes.Refresh:
                {
                    builder = (context, animation, secondaryAnimation) => new RefreshListScreen();
                    break;
                }
                default:
                    builder = null;
                    break;
            }

            return new PageRouteBuilder(
                settings,
                builder
            );
        }

        private static Route OnUnknownRoute(RouteSettings settings)
        {
            return new PageRouteBuilder(
                settings,
                (context, animation, secondaryAnimation) => new Container(
                    color: Colors.white,
                    child: new Center(
                        child: new Text("NOT FOUND 404")
                    )
                )
            );
        }
    }

    internal static class NavigatorRoutes
    {
        public const string Root = "/";
        public const string Page = "/page";
        public const string Refresh = "/refresh";
    }
}