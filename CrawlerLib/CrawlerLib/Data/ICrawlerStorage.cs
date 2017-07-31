// <copyright file="ICrawlerStorage.cs" company="DECTech.Tokyo">
// Copyright (c) DECTech.Tokyo. All rights reserved.
// </copyright>

namespace CrawlerLib.Data
{
    using System.Collections.Async;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;
    using JetBrains.Annotations;

    /// <summary>
    /// Interface for crawler data
    /// </summary>
    public interface ICrawlerStorage
    {
        /// <summary>
        /// Stores collection of referers for specific URI.
        /// </summary>
        /// <param name="sessionId">Crawling session.</param>
        /// <param name="uri">URI for which referer to store.</param>
        /// <param name="referer">Collection of referers.</param>
        /// <returns>A <see cref="Task" /> representing the asynchronous operation.</returns>
        Task AddPageReferer(string sessionId, string uri, string referer);

        /// <summary>
        /// Create new crawling session.
        /// </summary>
        /// <param name="rootUris">URIs used to start crawling.</param>
        /// <returns>New session Id.</returns>
        Task<string> CreateSession(IEnumerable<string> rootUris);

        /// <summary>
        /// Stores page content.
        /// </summary>
        /// <param name="uri">URI of page to store.</param>
        /// <param name="content">Stream for content to store.</param>
        /// <param name="cancellation">Operation cancellation.</param>
        /// <param name="metadata">Metadata name and value pairs.</param>
        /// <returns>A <see cref="Task" /> representing the asynchronous operation.</returns>
        Task DumpPage(
            string uri,
            Stream content,
            CancellationToken cancellation,
            IEnumerable<KeyValuePair<string, string>> metadata = null);

        /// <summary>
        /// Gets collection of all previous crawling sessions.
        /// </summary>
        /// <returns>Collection of sessions.</returns>
        [UsedImplicitly]
        Task<IEnumerable<ISessionInfo>> GetAllSessions();

        /// <summary>
        /// Gets collection of metadata names used in indexed documents.
        /// </summary>
        /// <returns>Collection of metadata names.</returns>
        Task<IEnumerable<string>> GetAvailableMetadata();

        /// <summary>
        /// Returns collection of URI referers.
        /// </summary>
        /// <param name="sessionId">Crawling session.</param>
        /// <param name="uri">URI whose referes to return.</param>
        /// <returns>Collection of referers.</returns>
        [UsedImplicitly]
        Task<IEnumerable<string>> GetReferers(string sessionId, string uri);

        /// <summary>
        /// Returns all URIs in crawling session.
        /// </summary>
        /// <param name="sessionId">Crawling session.</param>
        /// <returns>Collecion of URI strings.</returns>
        [UsedImplicitly]
        Task<IEnumerable<string>> GetSessionUris(string sessionId);

        /// <summary>
        /// Returns content of crawled URI.
        /// </summary>
        /// <param name="uri">Crawled URI.</param>
        /// <param name="destination">Content destination stream</param>
        /// <param name="cancellation">Download cancellation</param>
        /// <returns>Stream with content.</returns>
        Task GetUriContet(string uri, Stream destination, CancellationToken cancellation);

        /// <summary>
        /// Search blobs by metadata
        /// </summary>
        /// <param name="query">Collection of operators combined together as AND.</param>
        /// <param name="cancellation">Search cancellation.</param>
        /// <returns>Collection of URIs which metadata has that values.</returns>
        Task<IAsyncEnumerable<string>> SearchByMeta(IEnumerable<SearchCondition> query, CancellationToken cancellation);

        /// <summary>
        /// Search URLs content by free text
        /// </summary>
        /// <param name="text">Text to search.</param>
        /// <param name="cancellation">Search cancellation.</param>
        /// <returns>Collection of URIs which content has that text.</returns>
        Task<IAsyncEnumerable<string>> SearchByText(string text, CancellationToken cancellation);

        /// <summary>
        /// Store page crawling status code
        /// </summary>
        /// <param name="uri">Page URI.</param>
        /// <param name="code">Page status code.</param>
        /// <returns>A <see cref="Task" /> representing the asynchronous operation.</returns>
        Task StorePageError(string uri, HttpStatusCode code);
    }
}