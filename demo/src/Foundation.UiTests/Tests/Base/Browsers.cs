using System.Collections.Generic;

namespace Foundation.UiTests.Tests.Base
{
    public class Browsers
    {
        public enum Browser
        {
            Chrome,
            ChromeMobile,
            Firefox,
            InternetExplorer,
            Edge,
            Opera,
            Safari
        }

        public static Dictionary<Browser, string> BrowserNames =>
            new Dictionary<Browser, string>
            {
                {Browser.Chrome, "chrome"},
                {Browser.ChromeMobile, "chrome"},
                {Browser.Firefox, "firefox"},
                {Browser.Edge, "edge"},
                {Browser.InternetExplorer, "internetexplorer"},
                {Browser.Opera, "opera"},
                {Browser.Safari, "safari"},
            };
    }
}
