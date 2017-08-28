// <copyright file="Configuration.cs" company="DECTech.Tokyo">
// Copyright (c) DECTech.Tokyo. All rights reserved.
// </copyright>

namespace CrawlerLib
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Reflection;
    using System.Threading;
    using Data;
    using Grabbers;
    using JetBrains.Annotations;
    using Logger;
    using Metadata;

    /// <summary>
    /// Crawler configuration.
    /// </summary>
    public class Configuration
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Configuration" /> class.
        /// </summary>
        public Configuration()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Configuration" /> class.
        /// Clones configuration.
        /// </summary>
        /// <param name="config">Configuration to clone.</param>
        public Configuration(Configuration config)
        {
            if (config != null)
            {
                foreach (var prop in typeof(Configuration).GetRuntimeProperties())
                {
                    prop.SetMethod.Invoke(this, new[] { prop.GetMethod.Invoke(config, new object[0]) });
                }
            }
        }

        /// <summary>
        /// Gets or sets crawler cancellation token.
        /// </summary>
        public CancellationToken CancellationToken { get; set; } = CancellationToken.None;

        /// <summary>
        /// Gets or sets how deep crawler show fallow links from the initial URIs.
        /// 0 means it will crawl only initial URIs.
        /// </summary>
        public int Depth { get; set; } = 0;

        /// <summary>
        /// Gets or sets how deep crawler show fallow links different hosts from the initial URIs.
        /// 0 means it will crawl only hosts in initial URIs.
        /// </summary>
        public int HostDepth { get; set; } = 0;

        /// <summary>
        /// Gets or sets delay between crawling the same host. Other hosts can be crawled without delay.
        /// </summary>
        public TimeSpan HostRequestsDelay { get; set; } = TimeSpan.FromSeconds(1);

        /// <summary>
        /// Gets or sets grabber to dump page content.
        /// </summary>
        public HttpGrabber HttpGrabber { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether tells crawler to try to keep HTTP Referer while crawling.
        /// </summary>
        [UsedImplicitly]
        public bool KeepReferer { get; set; } = true;

        /// <summary>
        /// Gets or sets logger to use by crawler.
        /// </summary>
        public ILogger Logger { get; set; }

        /// <summary>
        /// Gets or sets collection of metadata extractors.
        /// </summary>
        public IList<IMetadataExtractor> MetadataExtractors { get; set; } = new IMetadataExtractor[]
                                                                            {
                                                                                new MicrodataMetadataExtractor(),
                                                                                new RdfaMetadataExtractor()
                                                                            };

        /// <summary>
        /// Gets or sets number of request to do same time.
        /// </summary>
        public int NumberOfSimulataneousRequests { get; set; } = 8;

        /// <summary>
        /// Gets or sets queue managing parsing jobs.
        /// </summary>
        public IParserJobsQueue Queue { get; set; }

        /// <summary>
        /// Gets or sets delay before retrying error.
        /// </summary>
        public TimeSpan RequestErrorRetryDelay { get; set; } = TimeSpan.FromSeconds(30);

        /// <summary>
        /// Gets or sets timeout for page grabber.
        /// </summary>
        public TimeSpan RequestTimeout { get; set; } = TimeSpan.FromSeconds(10);

        /// <summary>
        /// Gets or sets how manytimes crawler should try to open page in case of HTTP errors.
        /// </summary>
        public int RetriesNumber { get; set; } = 3;

        /// <summary>
        /// Gets or sets collection of HTTP error to retry.
        /// </summary>
        public IList<HttpStatusCode> RetryErrors { get; set; } =
            new[]
            {
                HttpStatusCode.InternalServerError,
                HttpStatusCode.GatewayTimeout,
                HttpStatusCode.BadGateway
            };

        /// <summary>
        /// Gets or sets storage used by crawler for dumping pages and keeping state.
        /// </summary>
        public ICrawlerStorage Storage { get; set; } = new DummyStorage();

        /// <summary>
        /// Gets or sets user-Agent used by crawler.
        /// </summary>
        public string UserAgent { get; set; } =
            "Mozilla/5.0 (Linux; Android 4.0.4; Galaxy Nexus Build/IMM76B) AppleWebKit/535.19 (KHTML, like Gecko) Chrome/18.0.1025.133 Mobile Safari/535.19";
    }
}