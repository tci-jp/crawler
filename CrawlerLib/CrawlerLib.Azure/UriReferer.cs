// <copyright file="UriReferer.cs" company="DECTech.Tokyo">
// Copyright (c) DECTech.Tokyo. All rights reserved.
// </copyright>

namespace CrawlerLib.Azure
{
    using global::Azure.Storage;
    using Microsoft.WindowsAzure.Storage.Table;

    [Table("referers")]
    public class UriReferer : TableEntity
    {
        public UriReferer()
        {
        }

        public UriReferer(string sessionId, string uri, string referer)
            : base(DataStorage.EncodeString(sessionId + "|" + uri), DataStorage.EncodeString(referer))
        {
        }

        public string Referer => DataStorage.DecodeString(RowKey);
    }
}