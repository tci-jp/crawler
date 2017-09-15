// <copyright file="ICrawler.cs" company="DECTech.Tokyo">
// Copyright (c) DECTech.Tokyo. All rights reserved.
// </copyright>

namespace CrawlerLib
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Data;
    using Queue;

    /// <summary>
    /// Base Crawler interface
    /// </summary>
    public interface ICrawler : IDisposable
    {
        /// <summary>
        /// Called when crawler parsed and dumped new page
        /// </summary>
        event Action<Crawler, string> UriCrawled;

        /// <summary>
        /// Starts crawling by collection of URIs
        /// </summary>
        /// <param name="ownerId">Session owner id.</param>
        /// <param name="uris">URIs to crawl.</param>
        /// <param name="cancellationTime">UTC time after which processing should be stopped. Unlimited if null.</param>
        /// <param name="parserParameters">Parsing parameters.</param>
        /// <returns>Session Id. A <see cref="Task" /> representing the asynchronous operation.</returns>
        Task<string> InciteStart(string ownerId, IEnumerable<UriParameter> uris, DateTime? cancellationTime, ParserParameters parserParameters = null);

        /// <summary>
        /// Run workers to parse queue.
        /// </summary>
        /// <param name="workers">Number of workers</param>
        void RunParserWorkers(int workers);

        /// <summary>
        /// Run one job
        /// </summary>
        /// <param name="job">Job to crawl</param>
        /// <param name="cancellation">Job cancellation.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task InciteJob(ICommitableParserJob job, CancellationToken cancellation);
    }
}