// <copyright file="CrawlRecord.cs" company="DECTech.Tokyo">
// Copyright (c) DECTech.Tokyo. All rights reserved.
// </copyright>

namespace CrawlerLib.Azure
{
    using System;
    using System.Text;
    using global::Azure.Storage;
    using Microsoft.WindowsAzure.Storage.Table;

    [Table("common", PartitionKey = "url")]
    public class CrawlRecord : TableEntity
    {
        public CrawlRecord(string url)
            : base(null, DataStorage.EncodeString(url))
        {
        }

        public string Url => DataStorage.DecodeString(RowKey);

        public string Status { get; set; }

        public string BlobName => RowKey;
    }
}