using UIWidgetsApp.Redux.State;
using Unity.UIWidgets;
using Unity.UIWidgets.Redux;

namespace UIWidgetsApp.Redux
{
    public static class StoreProvider
    {
        private static Store<AppState> _store;

        public static Store<AppState> Store
        {
            get
            {
                if (_store != null) return _store;
                var middleware = new[]
                {
                    // ReduxLogging.create<AppState>(),
                    ReduxThunk.create<AppState>()
                };
                _store = new Store<AppState>(
                    AppReducer.Reduce,
                    AppState.InitialState(),
                    middleware
                );
                return _store;
            }
        }
    }
}