using static Foundation.UiTests.Tests.Base.Platforms;

namespace Foundation.UiTests.Tests.Base
{
    public class Executions
    {
        public enum Execution 
        {
            Local,
            Remote
        }

        public static Execution GetExecutionContext(Platform platform)
        {
            switch (platform)
            {
                case Platform.Windows:
                    return Execution.Local;

                default:
                    return Execution.Remote;
            }
        }
    }
}
