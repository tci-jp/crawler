// <copyright file="SessionUri.cs" company="DECTech.Tokyo">
// Copyright (c) DECTech.Tokyo. All rights reserved.
// </copyright>

namespace CrawlerLib.Azure
{
    using global::Azure.Storage;
    using Microsoft.WindowsAzure.Storage.Table;

    [Table("sessionuri")]
    public class SessionUri : TableEntity
    {
        public SessionUri()
        {
        }

        public SessionUri(string sessionId, string uri)
            : base(sessionId, DataStorage.EncodeString(uri))
        {
        }

        public string Uri => DataStorage.DecodeString(RowKey);
    }
}