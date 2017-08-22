// <copyright file="SessionUri.cs" company="DECTech.Tokyo">
// Copyright (c) DECTech.Tokyo. All rights reserved.
// </copyright>

namespace CrawlerLib.Azure
{
    using Data;
    using global::Azure.Storage;
    using Microsoft.WindowsAzure.Storage.Table;

    /// <inheritdoc cref="IUriState" />
    /// <summary>
    /// Keeps URIs found during crawling session.
    /// </summary>
    [Table("sessionuri")]
    public class SessionUri : TableEntity, IUriState
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SessionUri" /> class.
        /// </summary>
        public SessionUri()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SessionUri" /> class.
        /// </summary>
        /// <param name="sessionId">Crawler session Id.</param>
        /// <param name="uri">Found URI.</param>
        /// <param name="state">Crawling state as HTTP status code.</param>
        public SessionUri(string sessionId, string uri, int state)
            : base(sessionId, Codec.EncodeString(uri))
        {
            State = state;
        }

        /// <summary>
        /// Gets crawling session Id
        /// </summary>
        [PartitionKey]
        public string SessionId => PartitionKey;

        /// <inheritdoc />
        public int State { get; set; }

        /// <inheritdoc />
        public string Uri => Codec.DecodeString(RowKey);
    }
}