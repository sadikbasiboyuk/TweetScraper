using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using TweetScraper.JsonFiles;

namespace TweetScraper
{
    class Driver
    {
        private IWebDriver driverChrome;
        private IWebElement Element;
        private WebDriverWait waitZ, waitS, waitM, waitB;
        ReadOnlyCollection<IWebElement> Elements;
        IJavaScriptExecutor js;
        private readonly string url;

        public Driver(string url)
        {
            this.url = url;
            DriverSettings();
            SetURL(url);
        }

        // settings of Driver
        private void DriverSettings()
        {
            var chromeOptions = new ChromeOptions();
            chromeOptions.AddArgument("--headless"); // Open browser in headless mode. Comment this line if you want to see browser
            chromeOptions.AddUserProfilePreference("profile.default_content_setting_values.images", 2); // Disables images for fast loading
            driverChrome = new ChromeDriver(@"Write your test driver's path here", chromeOptions); // You must define your chrome's path
            driverChrome.Manage().Window.Maximize(); // Maximize the window
            // Wait time for elements
            waitZ = new WebDriverWait(driverChrome, new TimeSpan(0, 0, 0, 0, 10));
            waitS = new WebDriverWait(driverChrome, new TimeSpan(0, 0, 3));
            waitM = new WebDriverWait(driverChrome, new TimeSpan(0, 0, 30));
            waitB = new WebDriverWait(driverChrome, new TimeSpan(0, 1, 0));
            js = (IJavaScriptExecutor)driverChrome;
            LogHolder.AddLogFile("Given url " + url + " driver is started", 1);
        }

        // Set Url of browser
        public bool SetURL(string url)
        {
            bool timeout = true;
            while (timeout)
            {
                int i = 0;
                try
                {
                    driverChrome.Navigate().GoToUrl(url);
                    return true;
                }
                catch (Exception e)
                {
                    // If cannot load, try to refresh 3 times
                    LogHolder.AddLogFile(url + " is cannot be loaded. It could be connection problem. Please check your connection." + e.Message, 1);
                    i++;
                    if (i < 3)
                    {
                        if (RefreshDriver())
                            return true;
                        else
                            timeout = true;
                    }
                    else
                        timeout = false;
                }
            }
            return false;
        }

        // Refreshing browser
        public bool RefreshDriver()
        {
            try
            {
                driverChrome.Navigate().Refresh();
                string x = driverChrome.Url;
                LogHolder.AddLogFile("Given address page is refreshed: " + url, 1);
                return true;
            }
            catch (Exception e)
            {
                LogHolder.AddLogFile("Given address page cannot be refreshed: " + url + e.Message, 1);
                return false;
            }
        }

        // Quit from driver, deletes cookies
        public bool DriverQuit()
        {
            try
            {
                driverChrome.Quit();
                LogHolder.AddLogFile("Operation is successful. Quitted from driver", 1);
                return true;
            }
            catch (Exception e)
            {
                LogHolder.AddLogFile("Cannot quit from driver. See error message for detail information: " + e.Message, 1);
                return false;
            }
        }

        // Check the element is available or not
        public bool CheckElementAvailable(string xPathElement, int choose, string xPathSite = "NoWebSite")
        {
            if (xPathSite != "NoWebSite")
                xPathElement = ParseJson.FindXpath(xPathSite, xPathElement);
            try
            {
                if (choose == -1)
                    Element = waitZ.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementExists(By.XPath(xPathElement)));
                else if (choose == 0)
                    Element = waitS.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementExists(By.XPath(xPathElement)));
                else if (choose == 1)
                    Element = waitM.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementExists(By.XPath(xPathElement)));
                else if (choose == 2)
                    Element = waitB.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementExists(By.XPath(xPathElement)));
            }
            catch (Exception)
            {
                return false;
            }

            if (Element == null)
                return false;

            return true;
        }

        // Click element in Browser
        public bool ClickElement(string xPathElement, string xPathSite = "NoWebSite")
        {
            if (xPathSite != "NoWebSite")
                xPathElement = ParseJson.FindXpath(xPathSite, xPathElement);
            try
            {
                Element = driverChrome.FindElement(By.XPath(xPathElement));
                Element.Click();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        // Fill box in browser
        public bool FillElement(string xPathElement, string elementName, string xPathSite = "NoWebSite")
        {
            if (xPathSite != "NoWebSite")
                xPathElement = ParseJson.FindXpath(xPathSite, xPathElement);
            try
            {
                Element = driverChrome.FindElement(By.XPath(xPathElement));
                Element.SendKeys(elementName);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        // Find elements that have given XPath, and return this list
        public ReadOnlyCollection<IWebElement> FindElements(string xPathElement, string xPathSite = "NoWebSite")
        {
            if (xPathSite != "NoWebSite")
                xPathElement = ParseJson.FindXpath(xPathSite, xPathElement);
            try
            {
                Elements = driverChrome.FindElements(By.XPath(xPathElement));
                return Elements;
            }
            catch (Exception)
            {
                return null;
            }
        }

        // Scroll Down with ration of body's quarter height, using directly body's height will cause many tweets to be overlooked
        public int TwitterScrollDown(int k)
        {
            var firstHeight = js.ExecuteScript("return document.body.scrollHeight");
            js.ExecuteScript("window.scrollBy(0,(document.body.scrollHeight)/4)");
            if (this.CheckElementAvailable("PageLoading", -1, "Twitter"))
                return 0;
            var secondHeight = js.ExecuteScript("return document.body.scrollHeight");
            if (firstHeight.Equals(secondHeight))
                return ++k;
            return 0;
        }

    }
}
