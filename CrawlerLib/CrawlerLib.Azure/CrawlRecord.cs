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
    [Table("common", PartitionKey = "url")]
    public class CrawlRecord : TableEntity
    {
        /// <inheritdoc />
        /// <summary>
        /// Initializes a new instance of the <see cref="CrawlRecord"/> class.
        /// </summary>
        /// <param name="uri">Crawled URI.</param>
        public CrawlRecord(string uri)
            : base(null, DataStorage.EncodeString(uri))
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
        public string BlobName => RowKey;
    }
}