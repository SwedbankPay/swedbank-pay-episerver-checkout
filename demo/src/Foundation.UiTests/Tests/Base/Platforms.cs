using System.Collections.Generic;

namespace Foundation.UiTests.Tests.Base
{
    public class Platforms
    {
        public enum Platform
        {
            Windows,
            Osx,
            Android,
            Ios
        }

        public static Dictionary<Platform, string> PlatformNames =>
            new Dictionary<Platform, string>
            {
                {Platform.Windows, "Windows 10"},
                {Platform.Osx, "Mac"},
                {Platform.Android, "Android"},
                {Platform.Ios, "iOS"}
            };
    }
}
