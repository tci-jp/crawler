// <copyright file="SessionInfo.cs" company="DECTech.Tokyo">
// Copyright (c) DECTech.Tokyo. All rights reserved.
// </copyright>

namespace CrawlerLib.Azure
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Data;
    using global::Azure.Storage;
    using Microsoft.WindowsAzure.Storage.Table;

    /// <inheritdoc cref="ComplexTableEntity" />
    [Table("sessions")]
    public class SessionInfo : ComplexTableEntity, ISessionInfo
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SessionInfo" /> class.
        /// </summary>
        public SessionInfo()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SessionInfo" /> class.
        /// </summary>
        /// <param name="ownerId">Owner Id.</param>
        /// <param name="sessionId">Session Id.</param>
        public SessionInfo(string ownerId, string sessionId)
            : base(ownerId, sessionId)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SessionInfo" /> class.
        /// Created Session object to store in Storage.
        /// </summary>
        /// <param name="ownerId">Session owner Id.</param>
        /// <param name="rootUris">List of URIs to crawl.</param>
        public SessionInfo(string ownerId, IEnumerable<string> rootUris)
            : this(ownerId, Guid.NewGuid().ToString(), rootUris)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SessionInfo" /> class.
        /// </summary>
        /// <param name="ownerId">Session owner id.</param>
        /// <param name="id">Session id.</param>
        /// <param name="rootUris">List if uris crawled in the session.</param>
        private SessionInfo(string ownerId, string id, IEnumerable<string> rootUris)
            : base(ownerId, id)
        {
            RootUris = rootUris.ToList();
        }

        /// <inheritdoc />
        [RowKey]
        public string Id => RowKey;

        /// <inheritdoc />
        [PartitionKey]
        public string OwnerId => PartitionKey;

        /// <inheritdoc />
        public IList<string> RootUris { get; set; }

        /// <inheritdoc />
        [IgnoreProperty]
        public SessionState State { get; set; }

        /// <summary>
        /// Gets or sets string representation for State
        /// </summary>
        public string StateString
        {
            get => State.ToString();
            set => State = (SessionState)Enum.Parse(typeof(SessionState), value);
        }

        /// <inheritdoc />
        public new DateTime Timestamp => base.Timestamp.UtcDateTime;
    }
}