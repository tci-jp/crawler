namespace CrawlerLib.Azure
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Data;
    using global::Azure.Storage;
    using Microsoft.WindowsAzure.Storage.Table;

    [Table("referers")]
    public class UriReferer : TableEntity
    {
        public UriReferer()
        {
        }

        public UriReferer(string sessionId, string uri, string referer)
            : base(sessionId + "|" + uri, referer)
        {
        }

        public string Referer => RowKey;
    }

    [Table("sessionuri")]
    public class SessionUri : TableEntity
    {
        public SessionUri()
        {
        }

        public SessionUri(string sessionId, string uri)
            : base(sessionId, uri)
        {
        }

        public string Uri => RowKey;
    }

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

        public IList<string> RootUris { get; }
    }
}