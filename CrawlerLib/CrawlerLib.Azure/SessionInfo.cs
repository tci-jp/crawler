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

    /// <inheritdoc cref="ComplexTableEntity"/>
    [Table("common", PartitionKey = "session")]
    public class SessionInfo : ComplexTableEntity, ISessionInfo
    {
        /// <inheritdoc />
        public SessionInfo()
        {
        }

        /// <inheritdoc />
        /// <summary>
        /// Initializes a new instance of the <see cref="SessionInfo" /> class.
        /// Created Session object to store in Storage.
        /// </summary>
        /// <param name="rootUris">List of URIs to crawl.</param>
        public SessionInfo(IEnumerable<string> rootUris)
            : this(Guid.NewGuid().ToString(), rootUris)
        {
        }

        /// <inheritdoc />
        private SessionInfo(string id, IEnumerable<string> rootUris)
            : base(null, id)
        {
            RootUris = rootUris.ToList();
        }

        /// <inheritdoc />
        public string Id => RowKey;

        /// <inheritdoc />
        public new DateTime Timestamp => base.Timestamp.UtcDateTime;

        /// <inheritdoc />
        public IList<string> RootUris { get; set; }
    }
}