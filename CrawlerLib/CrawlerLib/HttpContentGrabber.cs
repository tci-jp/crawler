using System.Net;

namespace CrawlerLib
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using OpenQA.Selenium;
    using OpenQA.Selenium.PhantomJS;
    using OpenQA.Selenium.Support.UI;

    /// <summary>
    ///         Execute JavaScript using phantomJS
    /// </summary>
    public class HttpContentGrabber : HttpGrabber
    {
        /// <summary>
        ///         Initializes a new instance of the <see cref="HttpContentGrabber"/> class.
        ///         Execute JavaScript using phantomJS
        /// </summary>
        /// <param name="config">" "</param>
        public HttpContentGrabber(Configuration config)
            : base(config)
        {
            this._webDriver = new PhantomJSDriver();
        }

        /// <summary>
        ///         Finalizes an instance of the <see cref="HttpContentGrabber"/> class.
        ///         clear _webDriver
        /// </summary>
        ~HttpContentGrabber()
        {
            this._webDriver.Quit();
        }

        /// <inheritdoc />
        public override Task<GrabResult> Grab(Uri uri, Uri referer)
        {
            _webDriver.Navigate().GoToUrl(uri);

            IWait<IWebDriver> wait = new OpenQA.Selenium.Support.UI.WebDriverWait(_webDriver, TimeSpan.FromSeconds(30.00));
            wait.Until(driver1 => ((IJavaScriptExecutor)_webDriver).ExecuteScript("return document.readyState").Equals("complete"));

            GrabResult pageContent = new GrabResult();

            pageContent.Content = _webDriver.PageSource;
            pageContent.Status = HttpStatusCode.OK;

            return Task.FromResult<GrabResult>(pageContent);
        }

        private IWebDriver _webDriver;
    }
}
