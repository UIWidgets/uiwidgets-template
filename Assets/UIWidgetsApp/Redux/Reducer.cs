using UIWidgetsApp.Redux.Action;
using UIWidgetsApp.Redux.State;

namespace UIWidgetsApp.Redux
{
    public static class AppReducer
    {
        public static AppState Reduce(AppState state, object bAction)
        {
            switch (bAction)
            {
                case AddCountAction action:
                {
                    state.testState.count += action.added;
                    break;
                }
            }

            return state;
        }
    }
}