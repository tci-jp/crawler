// <copyright file="ICrawlerStorage.cs" company="DECTech.Tokyo">
// Copyright (c) DECTech.Tokyo. All rights reserved.
// </copyright>

namespace CrawlerLib.Data
{
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using System.Threading.Tasks;

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
        /// <returns>A <see cref="Task" /> representing the asynchronous operation.</returns>
        Task DumpPage(string uri, Stream content);

        /// <summary>
        /// Gets collection of all previous crawling sessions.
        /// </summary>
        /// <returns>Collection of sessions.</returns>
        Task<IEnumerable<SessionInfo>> GetAllSessions();

        /// <summary>
        /// Returns collection of URI referers.
        /// </summary>
        /// <param name="sessionId">Crawling session.</param>
        /// <param name="uri">URI whose referes to return.</param>
        /// <returns>Collection of referers.</returns>
        Task<IEnumerable<string>> GetReferers(string sessionId, string uri);

        /// <summary>
        /// Returns all URIs in crawling session.
        /// </summary>
        /// <param name="sessionId">Crawling session.</param>
        /// <returns>Collecion of URI strings.</returns>
        Task<IEnumerable<string>> GetSessionUris(string sessionId);

        /// <summary>
        /// Returns content of crawled URI.
        /// </summary>
        /// <param name="uri">Crawled URI.</param>
        /// <returns>Stream with content.</returns>
        Task<Stream> GetUriContet(string uri);

        /// <summary>
        /// Search URLs content by free text
        /// </summary>
        /// <param name="text">Text to search.</param>
        /// <returns>Collection of URIs which content has that text.</returns>
        Task<IEnumerable<string>> SearchText(string text);

        /// <summary>
        /// Store page crawling status code
        /// </summary>
        /// <param name="sessionId">Crawling session.</param>
        /// <param name="uri">Page URI.</param>
        /// <param name="code">Page status code.</param>
        /// <returns>A <see cref="Task" /> representing the asynchronous operation.</returns>
        Task StorePageError(string sessionId, string uri, HttpStatusCode code);
    }
}