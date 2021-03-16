using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using static Foundation.UiTests.Tests.Base.Browsers;

namespace Foundation.UiTests.Services
{
    public static class WebDriverCleanerService
    {
        public static Dictionary<Browser, string> DriverNames =>
            new Dictionary<Browser, string>
            {
                {Browser.Chrome, "chromedriver"},
                {Browser.ChromeMobile, "chromedriver"},
                {Browser.Firefox, "geckodriver"},
                {Browser.Edge, null},
                {Browser.InternetExplorer, "IEDriverServer"},
                {Browser.Opera, null},
                {Browser.Safari, null},
            };

        public static void KillWebDriverProcess(string driverName)
        {
            try
            {
                if (driverName != null)
                {
                    Process.GetProcesses()
                        .Where(p => p.ProcessName == driverName)
                        .ToList()
                        .ForEach(p => p.Kill());
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}
