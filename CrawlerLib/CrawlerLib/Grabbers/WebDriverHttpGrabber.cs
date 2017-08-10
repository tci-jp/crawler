// <copyright file="WebDriverHttpGrabber.cs" company="DECTech.Tokyo">
// Copyright (c) DECTech.Tokyo. All rights reserved.
// </copyright>

namespace CrawlerLib.Grabbers
{
    using System;
    using System.Net;
    using System.Threading.Tasks;
    using JetBrains.Annotations;

    /// <inheritdoc />
    /// <summary>
    /// Execute JavaScript using phantomJS
    /// </summary>
    [UsedImplicitly]
    public class WebDriverHttpGrabber : HttpGrabber
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WebDriverHttpGrabber"/> class.
        /// </summary>
        /// <param name="config">Configuration for grabber.</param>
        public WebDriverHttpGrabber(Configuration config)
            : base(config)
        {
        }

        /// <inheritdoc />
        public override Task<GrabResult> Grab(Uri uri, Uri referer)
        {
            return Task.Run(
                async () =>
                {
                    using (var webDriver = new PhantomJSDriver())
                    {
                        var content = await webDriver.Grab(uri, Config.CancellationToken);

                        var pageContent = new GrabResult
                        {
                            Content = content,
                            Status = HttpStatusCode.OK
                        };
                        return pageContent;
                    }
                },
                Config.CancellationToken);
        }
    }
}