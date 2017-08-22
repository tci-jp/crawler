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
        /// <param name="ownerId">Id of session owner.</param>
        /// <param name="rootUris">URIs used to start crawling.</param>
        /// <returns>New session Id.</returns>
        Task<string> CreateSession(string ownerId, IEnumerable<string> rootUris);

        /// <summary>
        /// Create new crawling session.
        /// </summary>
        /// <param name="ownerId">Id of session owner.</param>
        /// <param name="sessionId">Crawling session Id.</param>
        /// <param name="state">Session state.</param>
        /// <returns>New session Id.</returns>
        Task UpdateSessionState(string ownerId, string sessionId, SessionState state);

        /// <summary>
        /// Stores page content.
        /// </summary>
        /// <param name="ownerId">Page owner id.</param>
        /// <param name="sessionId">Crawling Session Id.</param>
        /// <param name="uri">URI of page to store.</param>
        /// <param name="content">Stream for content to store.</param>
        /// <param name="cancellation">Operation cancellation.</param>
        /// <param name="metadata">Metadata name and value pairs.</param>
        /// <returns>A <see cref="Task" /> representing the asynchronous operation.</returns>
        Task DumpUriContent(
            string ownerId,
            string sessionId,
            string uri,
            Stream content,
            CancellationToken cancellation,
            IEnumerable<KeyValuePair<string, string>> metadata = null);

        /// <summary>
        /// Gets collection of metadata names used in indexed documents.
        /// </summary>
        /// <returns>Collection of metadata names.</returns>
        [UsedImplicitly]
        IAsyncEnumerable<string> GetAvailableMetadata();

        /// <summary>
        /// Returns collection of URI referers.
        /// </summary>
        /// <param name="sessionId">Crawling session.</param>
        /// <param name="uri">URI whose referes to return.</param>
        /// <returns>Collection of referers.</returns>
        [UsedImplicitly]
        IAsyncEnumerable<string> GetReferers(string sessionId, string uri);

        /// <summary>
        /// Gets collection of all previous crawling sessions.
        /// </summary>
        /// <param name="ownerId">Sessions owner Id.</param>
        /// <param name="sessionIds">
        /// Collection of session id to get info about.
        /// If null returns all sessions.
        /// </param>
        /// <param name="pageSize">Size of page.</param>
        /// <param name="requestId">Paged request id. Null for the beggining.</param>
        /// <param name="cancellation">Cancellation.</param>
        /// <returns>Collection of sessions.</returns>
        [UsedImplicitly]
        Task<IPage<ISessionInfo>> GetSessions(
            string ownerId,
            IEnumerable<string> sessionIds,
            int pageSize = 10,
            string requestId = null,
            CancellationToken cancellation = default(CancellationToken));

        /// <summary>
        /// Returns all URIs in crawling session.
        /// </summary>
        /// <param name="sessionId">Crawling session.</param>
        /// <returns>Collecion of URI strings.</returns>
        [UsedImplicitly]
        IAsyncEnumerable<IUriState> GetSessionUris(string sessionId);

        /// <summary>
        /// Returns content of crawled URI.
        /// </summary>
        /// <param name="ownerId">Blob owner Id.</param>
        /// <param name="uri">Crawled URI.</param>
        /// <param name="destination">Content destination stream</param>
        /// <param name="cancellation">Download cancellation</param>
        /// <returns>Stream with content.</returns>
        [UsedImplicitly]
        Task GetUriContet(string ownerId, string uri, Stream destination, CancellationToken cancellation = default(CancellationToken));

        /// <summary>
        /// Search blobs by metadata
        /// </summary>
        /// <param name="query">Collection of operators combined together as AND.</param>
        /// <param name="cancellation">Search cancellation.</param>
        /// <returns>Collection of URIs which metadata has that values.</returns>
        [UsedImplicitly]
        IAsyncEnumerable<string> SearchByMeta(IEnumerable<SearchCondition> query, CancellationToken cancellation = default(CancellationToken));

        /// <summary>
        /// Search URLs content by free text
        /// </summary>
        /// <param name="text">Text to search.</param>
        /// <param name="cancellation">Search cancellation.</param>
        /// <returns>Collection of URIs which content has that text.</returns>
        [UsedImplicitly]
        IAsyncEnumerable<string> SearchByText(string text, CancellationToken cancellation = default(CancellationToken));

        /// <summary>
        /// Store page crawling status code
        /// </summary>
        /// <param name="ownerId">Crawling owner id</param>
        /// <param name="sessionId">Crawling session id.</param>
        /// <param name="uri">Page URI.</param>
        /// <param name="code">Page status code.</param>
        /// <returns>A <see cref="Task" /> representing the asynchronous operation.</returns>
        Task StorePageError(string ownerId, string sessionId, string uri, HttpStatusCode code);

        /// <summary>
        /// Puts uri to sesion queue for crawling
        /// </summary>
        /// <param name="sessionId">Crawling session id.</param>
        /// <param name="uri">Page URI.</param>
        /// <returns>A <see cref="Task" /> representing the asynchronous operation.</returns>
        Task EnqueSessionUri(string sessionId, string uri);

        /// <summary>
        /// Updates uri crawling state
        /// </summary>
        /// <param name="sessionId">Crawling session id.</param>
        /// <param name="uri">Page URI.</param>
        /// <param name="statusCode">Crawling Http status code.</param>
        /// <returns>A <see cref="Task" /> representing the asynchronous operation.</returns>
        Task UpdateSessionUri(string sessionId, string uri, int statusCode);

        /// <summary>
        /// Gets metadata parsed from URI
        /// </summary>
        /// <param name="ownerId">Blob owner id.</param>
        /// <param name="uri">Page uri.</param>
        /// <param name="cancellation">Cancellation.</param>
        /// <returns>Async collection of key-value pairs of metadata field name and value.</returns>
        IAsyncEnumerable<KeyValuePair<string, string>> GetUriMetadata(string ownerId, string uri, CancellationToken cancellation = default(CancellationToken));
    }
}