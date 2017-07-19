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

    [Table("common", PartitionKey = "session")]
    public class SessionInfo : ComplexTableEntity, ISessionInfo
    {
        public SessionInfo()
        {
        }

        public SessionInfo(string id, IEnumerable<string> rootUris)
            : base(null, id)
        {
            RootUris = rootUris.ToList();
        }

        public SessionInfo(IEnumerable<string> rootUris)
            : this(Guid.NewGuid().ToString(), rootUris)
        {
        }

        public string Id => RowKey;

        public new DateTime Timestamp => base.Timestamp.UtcDateTime;

        public IList<string> RootUris { get; set; }
    }
}