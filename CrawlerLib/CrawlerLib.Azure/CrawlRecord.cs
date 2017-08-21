// <copyright file="CrawlRecord.cs" company="DECTech.Tokyo">
// Copyright (c) DECTech.Tokyo. All rights reserved.
// </copyright>

namespace CrawlerLib.Azure
{
    using global::Azure.Storage;
    using Microsoft.WindowsAzure.Storage.Table;

    /// <inheritdoc />
    /// <summary>
    /// Page dumping information.
    /// </summary>
    [Table("crawlrecords")]
    public class CrawlRecord : TableEntity
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CrawlRecord" /> class.
        /// </summary>
        /// <param name="ownerid">Blob owner Id.</param>
        /// <param name="uri">Crawled URI.</param>
        public CrawlRecord(string ownerid, string uri)
            : base(ownerid, DataStorage.EncodeString(uri))
        {
        }

        /// <summary>
        /// Gets crawled URI.
        /// </summary>
        public string Uri => DataStorage.DecodeString(RowKey);

        /// <summary>
        /// Gets or sets page dumping HTTP status.
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// Gets name of blob with dumped content.
        /// </summary>
        public string BlobName => PartitionKey + "/" + RowKey;

        /// <summary>
        /// Gets blob owner id.
        /// </summary>
        [PartitionKey]
        public string OwnerId => PartitionKey;
    }
}