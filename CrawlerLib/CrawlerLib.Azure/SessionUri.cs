// <copyright file="SessionUri.cs" company="DECTech.Tokyo">
// Copyright (c) DECTech.Tokyo. All rights reserved.
// </copyright>

namespace CrawlerLib.Azure
{
    using Data;
    using global::Azure.Storage;
    using Microsoft.WindowsAzure.Storage.Table;

    /// <inheritdoc />
    /// <summary>
    /// Keeps URIs found during crawling session.
    /// </summary>
    [Table("sessionuri")]
    public class SessionUri : TableEntity, IUriState
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SessionUri"/> class.
        /// </summary>
        public SessionUri()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SessionUri" /> class.
        /// </summary>
        /// <param name="sessionId">Crawler session Id.</param>
        /// <param name="uri">Found URI.</param>
        public SessionUri(string sessionId, string uri, int state)
            : base(sessionId, DataStorage.EncodeString(uri))
        {
            State = state;
        }

        /// <summary>
        /// Gets found URI.
        /// </summary>
        public string Uri => DataStorage.DecodeString(RowKey);

        /// <summary>
        /// Gets or sets crawling HTTP state
        /// </summary>
        public int State { get; set; }
    }
}