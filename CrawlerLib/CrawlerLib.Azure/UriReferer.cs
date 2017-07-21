// <copyright file="UriReferer.cs" company="DECTech.Tokyo">
// Copyright (c) DECTech.Tokyo. All rights reserved.
// </copyright>

namespace CrawlerLib.Azure
{
    using global::Azure.Storage;
    using Microsoft.WindowsAzure.Storage.Table;

    /// <inheritdoc />
    /// <summary>
    /// Keeps referer for crawled uri.
    /// </summary>
    [Table("referers")]
    public class UriReferer : TableEntity
    {
        /// <inheritdoc />
        public UriReferer()
        {
        }

        /// <inheritdoc />
        /// <summary>
        /// Initializes a new instance of the <see cref="UriReferer" /> class.
        /// Creates new entiry for session, uri and referer.
        /// </summary>
        /// <param name="sessionId">Session Id.</param>
        /// <param name="uri">Crawled URI.</param>
        /// <param name="referer">Crawler URI HTTP Referer.</param>
        public UriReferer(string sessionId, string uri, string referer)
            : base(DataStorage.EncodeString(sessionId + "|" + uri), DataStorage.EncodeString(referer))
        {
        }

        /// <summary>
        /// Gets HTTP Referer for crawled URI.
        /// </summary>
        public string Referer => DataStorage.DecodeString(RowKey);
    }
}