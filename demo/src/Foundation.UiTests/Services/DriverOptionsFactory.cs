using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.IE;
using OpenQA.Selenium.Safari;
using System;
using System.IO;
using static Foundation.UiTests.Tests.Base.Browsers;

namespace Foundation.UiTests.Services
{
    public static class DriverOptionsFactory
    {
        public static string DownloadFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Downloads");

        public static DriverOptions GetDriverOptions(Browser driver)
        {
            switch (driver)
            {
                case Browser.Chrome:

                    var chromeOptions = new ChromeOptions { AcceptInsecureCertificates = true };
                    chromeOptions.AddArguments("--incognito", "--disable-infobars", "--disable-notifications", "disable-extensions", "download.prompt_for_download=false", "--browser.download.dir=" + Path.GetFullPath(DownloadFolder));
                    chromeOptions.AddUserProfilePreference("download.default_directory", Path.GetFullPath(DownloadFolder));
                    chromeOptions.AddUserProfilePreference("profile.content_settings.exceptions.automatic_downloads.*.setting", 1);
                    chromeOptions.AddArguments("--lang=sv-SE");

                    /* Not working yet */
                    //ChromePerformanceLoggingPreferences perfLogPrefs = new ChromePerformanceLoggingPreferences();
                    //perfLogPrefs.AddTracingCategories(new string[] { "devtools.network", "devtools.timeline" });
                    //chromeOptions.PerformanceLoggingPreferences = perfLogPrefs;
                    //chromeOptions.AddAdditionalCapability("goog:loggingPrefs", true, true);
                    //chromeOptions.SetLoggingPreference("performance", LogLevel.All);

                    return chromeOptions;

                case Browser.ChromeMobile:

                    var chromeMobileOptions = new ChromeOptions { AcceptInsecureCertificates = true };
                    chromeMobileOptions.AddArguments("--incognito", "--disable-infobars", "--disable-notifications", "disable-extensions", "download.prompt_for_download=false", "--browser.download.dir=" + Path.GetFullPath(DownloadFolder));
                    chromeMobileOptions.EnableMobileEmulation("iPhone 8 Plus");
                    chromeMobileOptions.AddUserProfilePreference("download.default_directory", Path.GetFullPath(DownloadFolder));
                    chromeMobileOptions.AddUserProfilePreference("profile.content_settings.exceptions.automatic_downloads.*.setting", 1);

                    return chromeMobileOptions;

                case Browser.Firefox:

                    var firefoxOptions = new FirefoxOptions { AcceptInsecureCertificates = true };
                    firefoxOptions.AddArgument("-private");
                    firefoxOptions.SetPreference("dom.webnotifications.enabled", false);
                    firefoxOptions.SetPreference("browser.download.folderList", 2);
                    firefoxOptions.SetPreference("browser.download.manager.showWhenStarting", false);
                    firefoxOptions.SetPreference("browser.download.dir", Path.GetFullPath(DownloadFolder));
                    firefoxOptions.SetPreference("browser.download.useDownloadDir", true);
                    firefoxOptions.SetPreference("helperApps.alwaysAsk.force", false);
                    firefoxOptions.SetPreference("browser.helperApps.neverAsk.saveToDisk", "application/pdf,application/octet-stream");
                    firefoxOptions.SetPreference("browser.helperApps.neverAsk.openFile", true);
                    firefoxOptions.SetPreference("pdfjs.disabled", true);

                    return firefoxOptions;

                case Browser.InternetExplorer:

                    return new InternetExplorerOptions
                    {
                        AcceptInsecureCertificates = true,
                        BrowserCommandLineArguments = "",
                        EnsureCleanSession = true,
                        RequireWindowFocus = false,
                    };


                case Browser.Safari:

                    return new SafariOptions
                    {
                        AcceptInsecureCertificates = true
                    };

                default:

                    throw new NotFoundException("This driver is not in the list of handled web drivers");
            }
        }
    }
}
