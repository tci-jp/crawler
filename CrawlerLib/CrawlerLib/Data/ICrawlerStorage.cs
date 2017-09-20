// <copyright file="ICrawlerStorage.cs" company="DECTech.Tokyo">
// Copyright (c) DECTech.Tokyo. All rights reserved.
// </copyright>

namespace CrawlerLib.Data
{
    using System;
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
        /// Createsnew crawling session.
        /// </summary>
        /// <param name="ownerId">Id of session owner.</param>
        /// <param name="rootUris">URIs used to start crawling.</param>
        /// <param name="cancellationTime">Time after which crawling session should be stopped.</param>
        /// <returns>New session Id.</returns>
        Task<string> CreateSession(string ownerId, IEnumerable<string> rootUris, DateTime? cancellationTime = null);

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
        /// Puts uri to sesion queue for crawling
        /// </summary>
        /// <param name="sessionId">Crawling session id.</param>
        /// <param name="uri">Page URI.</param>
        /// <returns>A <see cref="Task" /> representing the asynchronous operation.</returns>
        Task<bool> EnqueSessionUri(string sessionId, string uri);

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
        /// Get info about single session.
        /// </summary>
        /// <param name="ownerId">Id of session owner.</param>
        /// <param name="sessionId">Crawling session Id.</param>
        /// <returns>Session information.</returns>
        Task<ISessionInfo> GetSingleSession(string ownerId, string sessionId);

        /// <summary>
        /// Returns content of crawled URI.
        /// </summary>
        /// <param name="ownerId">Blob owner Id.</param>
        /// <param name="uri">Crawled URI.</param>
        /// <param name="cancellation">Download cancellation</param>
        /// <returns>Stream with content.</returns>
        [UsedImplicitly]
        Task<Stream> GetUriContent(
            string ownerId,
            string uri,
            CancellationToken cancellation = default(CancellationToken));

        /// <summary>
        /// Gets metadata parsed from URI
        /// </summary>
        /// <param name="ownerId">Blob owner id.</param>
        /// <param name="uri">Page uri.</param>
        /// <param name="cancellation">Cancellation.</param>
        /// <returns>Async collection of key-value pairs of metadata field name and value.</returns>
        IAsyncEnumerable<KeyValuePair<string, string>> GetUriMetadata(
            string ownerId,
            string uri,
            CancellationToken cancellation = default(CancellationToken));

        /// <summary>
        /// Retreives single Parser Parameters
        /// </summary>
        /// <param name="ownerId">Parser Parameters ownerId</param>
        /// <param name="parserId">Parser Parameters Id</param>
        /// <returns>Parser Parameters object.</returns>
        Task<IParserParameters> RetreiveParserParametersAsync(string ownerId, string parserId);

        /// <summary>
        /// Search blobs by metadata
        /// </summary>
        /// <param name="query">Collection of operators combined together as AND.</param>
        /// <param name="cancellation">Search cancellation.</param>
        /// <returns>Collection of URIs which metadata has that values.</returns>
        [UsedImplicitly]
        IAsyncEnumerable<string> SearchByMeta(
            IEnumerable<SearchCondition> query,
            CancellationToken cancellation = default(CancellationToken));

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
        /// Store parser parameters. Using parserid and ownerid as unique key. Overwrite existing.
        /// </summary>
        /// <param name="parserParameters">Parser parameters</param>
        /// <returns>A <see cref="Task" /> representing the asynchronous operation.</returns>
        Task StoreParserParametersAsync(IParserParameters parserParameters);

        /// <summary>
        /// Updates Session Cancellation time. Can be set as current time to cancel as soon as possible.
        /// </summary>
        /// <param name="ownerId">Id of session owner.</param>
        /// <param name="sessionId">Crawling session Id.</param>
        /// <param name="cancellation">
        /// UTC time after which session procession should be stopped.
        /// If null procession is not limited
        /// </param>
        /// <returns>A <see cref="Task" /> representing the asynchronous operation.</returns>
        Task UpdateSessionCancellation(string ownerId, string sessionId, DateTime? cancellation);

        /// <summary>
        /// Createsnew crawling session.
        /// </summary>
        /// <param name="ownerId">Id of session owner.</param>
        /// <param name="sessionId">Crawling session Id.</param>
        /// <param name="state">Session state.</param>
        /// <returns>New session Id.</returns>
        Task UpdateSessionState(string ownerId, string sessionId, SessionState state);

        /// <summary>
        /// Updates uri crawling state
        /// </summary>
        /// <param name="sessionId">Crawling session id.</param>
        /// <param name="uri">Page URI.</param>
        /// <param name="statusCode">Crawling Http status code.</param>
        /// <param name="message">Error message.</param>
        /// <returns>A <see cref="Task" /> representing the asynchronous operation.</returns>
        Task UpdateSessionUri(string sessionId, string uri, int statusCode, string message = null);

        /// <summary>
        /// Retreives all Parser Parameters
        /// </summary>
        /// <param name="ownerId">Parser Parameters ownerId</param>
        /// <returns>Parser Parameters objects.</returns>
        IAsyncEnumerable<IParserParameters> RetreiveAllParserParametersAsync(string ownerId);
    }
}