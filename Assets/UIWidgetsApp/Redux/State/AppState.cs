namespace UIWidgetsApp.Redux.State
{
    public class AppState
    {
        public TestState testState { get; set; }

        public static AppState InitialState()
        {
            return new AppState
            {
                testState = new TestState
                {
                    count = 0
                }
            };
        }
    }

    public class TestState
    {
        public int count;
    }
}