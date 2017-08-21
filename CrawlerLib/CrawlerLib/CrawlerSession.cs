// <copyright file="CrawlerSession.cs" company="DECTech.Tokyo">
// Copyright (c) DECTech.Tokyo. All rights reserved.
// </copyright>

namespace CrawlerLib
{
    using System.Threading.Tasks;

    /// <summary>
    /// Crawler session info.
    /// </summary>
    public class CrawlerSession
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CrawlerSession" /> class.
        /// </summary>
        /// <param name="sessionId">Session Id.</param>
        /// <param name="crawlerTask">Crawling Task.</param>
        public CrawlerSession(string sessionId, Task crawlerTask)
        {
            SessionId = sessionId;
            CrawlerTask = crawlerTask;
        }

        /// <summary>
        /// Gets crawling Task.
        /// </summary>
        public Task CrawlerTask { get; }

        /// <summary>
        /// Gets session Id.
        /// </summary>
        public string SessionId { get; }
    }
}