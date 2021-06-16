using System.Collections;

namespace Foundation.UiTests.Tests.Base
{
    public class Profiles
    {
        public class ProfileRELEASE : IEnumerable
        {
            public IEnumerator GetEnumerator()
            {
                yield return new object[] { Platforms.Platform.Windows, Devices.Device.NotApplicable,   Browsers.Browser.Chrome};
                yield return new object[] { Platforms.Platform.Windows, Devices.Device.NotApplicable,   Browsers.Browser.Firefox};
            }
        }
    }
}
