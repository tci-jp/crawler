// <copyright file="WebDriverHttpGrabber.cs" company="DECTech.Tokyo">
// Copyright (c) DECTech.Tokyo. All rights reserved.
// </copyright>

namespace CrawlerLib.Grabbers
{
    using System;
    using System.Net;
    using System.Threading.Tasks;
    using OpenQA.Selenium;
    using OpenQA.Selenium.PhantomJS;
    using OpenQA.Selenium.Support.UI;

    /// <inheritdoc />
    /// <summary>
    /// Execute JavaScript using phantomJS
    /// </summary>
    public class WebDriverHttpGrabber : HttpGrabber
    {
        /// <inheritdoc />
        public WebDriverHttpGrabber(Configuration config)
            : base(config)
        {
        }

        /// <inheritdoc />
        public override Task<GrabResult> Grab(Uri uri, Uri referer)
        {
            return Task.Run(
                () =>
                {
                    using (var driverService = PhantomJSDriverService.CreateDefaultService())
                    {
                        driverService.HideCommandPromptWindow = true;
                        using (var webDriver = new PhantomJSDriver(driverService))
                        {
                            webDriver.Navigate().GoToUrl(uri);

                            IWait<IWebDriver> wait = new WebDriverWait(webDriver, Config.RequestTimeout);

                            wait.Until(driver => ((IJavaScriptExecutor)driver)
                                                 .ExecuteScript("return document.readyState")
                                                 .Equals("complete")
                                                 || Config.CancellationToken.IsCancellationRequested);

                            var pageContent = new GrabResult
                                              {
                                                  Content = webDriver.PageSource,
                                                  Status = HttpStatusCode.OK
                                              };
                            return pageContent;
                        }
                    }
                },
                Config.CancellationToken);
        }
    }
}