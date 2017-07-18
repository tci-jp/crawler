namespace CrawlerLib.Grabbers
{
    using System;
    using System.Collections.Concurrent;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;
    using OpenQA.Selenium;
    using OpenQA.Selenium.PhantomJS;
    using OpenQA.Selenium.Support.UI;

    /// <summary>
    /// Execute JavaScript using phantomJS
    /// </summary>
    public class HttpContentGrabber : HttpGrabber
    {
        public HttpContentGrabber(Configuration config) : base(config)
        {
        }

        /// <inheritdoc/>
        public override Task<GrabResult> Grab(Uri uri, Uri referer)
        {
            return Task.Run(
                () =>
                    {
                        var webDriver = CreateNewWebDriver();

                        try
                        {
                            webDriver.Navigate().GoToUrl(uri);

                            IWait<IWebDriver> wait = new WebDriverWait(webDriver, TimeSpan.FromSeconds(30.00));
                            wait.Until(driver1 => ((IJavaScriptExecutor)webDriver)
                                           .ExecuteScript("return document.readyState").Equals("complete")
                                           || Config.CancellationToken.IsCancellationRequested);

                            var pageContent = new GrabResult()
                            {
                                Content = webDriver.PageSource,
                                Status = HttpStatusCode.OK
                            };
                            return pageContent;
                        }
                        finally
                        {
                            webDriver.Quit();
                        }
                    },
                Config.CancellationToken);
        }

        private IWebDriver CreateNewWebDriver()
        {
            var driverService = PhantomJSDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;
            return new PhantomJSDriver(driverService);
        }
    }
}