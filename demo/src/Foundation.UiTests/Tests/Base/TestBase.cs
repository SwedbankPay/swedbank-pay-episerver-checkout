using Atata;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.IE;
using System;
using Foundation.UiTests.Services;

namespace Foundation.UiTests.Tests.Base
{
    #if DEBUG
    [TestFixture(Browsers.Browser.Chrome)]
    #elif RELEASE
    [TestFixture(Browsers.Browser.Chrome)]
    #endif
    public abstract class TestBase
    {
        protected readonly Browsers.Browser _browser;

        [OneTimeSetUp]
        public void GlobalSetup()
        {
            AtataContext.GlobalConfiguration.
                UseChrome().
                    WithOptions(DriverOptionsFactory.GetDriverOptions(Browsers.Browser.Chrome) as ChromeOptions).
                UseFirefox().
                    WithOptions(DriverOptionsFactory.GetDriverOptions(Browsers.Browser.Firefox) as FirefoxOptions).
                UseInternetExplorer().
                    WithOptions(DriverOptionsFactory.GetDriverOptions(Browsers.Browser.InternetExplorer) as InternetExplorerOptions).
                AddNUnitTestContextLogging().
                WithMinLevel(Atata.LogLevel.Trace).
                TakeScreenshotOnNUnitError().
                    AddScreenshotFileSaving().
                        WithFolderPath(() => $@"Logs\{AtataContext.BuildStart:yyyy-MM-dd HH_mm_ss}").
                        WithFileName(screenshotInfo => $"{AtataContext.Current.TestName} - {screenshotInfo.PageObjectFullName}").
                UseTestName(() => $"[{Browsers.BrowserNames[_browser]}]{TestContext.CurrentContext.Test.Name}");
        }

        [SetUp]
        public void SetUp()
        {
            switch (_browser)
            {
                case Browsers.Browser.ChromeMobile:
                    AtataContext.Configure()
                        .UseChrome()
                            .WithOptions(DriverOptionsFactory.GetDriverOptions(Browsers.Browser.ChromeMobile) as ChromeOptions)
                    .Build();
                    break;

                default:
                    AtataContext.Configure()
                        .UseDriver(Browsers.BrowserNames[_browser])
                    .Build();
                    AtataContext.Current.Driver.Maximize();
                    break;
            }
        }

        public TestBase(Browsers.Browser browser)
        {
            _browser = browser;
        }

        [OneTimeTearDown]
        public void GlobalDown()
        {
            foreach (Browsers.Browser driverType in Enum.GetValues(typeof(Browsers.Browser)))
            {
                WebDriverCleanerService.KillWebDriverProcess(WebDriverCleanerService.DriverNames[driverType]);
            }
        }

        [TearDown]
        public void TearDown()
        {
            AtataContext.Current?.CleanUp();
        }

        public static T GoTo<T>(bool cookies = true) where T : PageObject<T>
        {
            var page = Go.To<T>().Wait(3);

            if (cookies)
            {
                if (AtataContext.Current.Driver.FindElements(By.ClassName("jsCookiesBtn")).Count == 1)
                {
                    if (AtataContext.Current.Driver.FindElementByClassName("jsCookiesBtn") is IWebElement element && element.Displayed)
                    {
                        element.Click();
                    }
                }
            }
            
            return page;
        }
    }
}
