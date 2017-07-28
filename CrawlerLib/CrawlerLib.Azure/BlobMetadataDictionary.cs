// <copyright file="BlobMetadataDictionary.cs" company="DECTech.Tokyo">
// Copyright (c) DECTech.Tokyo. All rights reserved.
// </copyright>

namespace CrawlerLib.Azure
{
    using System.Collections.Generic;
    using global::Azure.Storage;

    /// <inheritdoc />
    /// <summary>
    /// Set of metadata from Blob
    /// </summary>
    [Table("blobmetadata", PartitionKey = "blobmetadata")]
    public class BlobMetadataDictionary : DictionaryTableEntity
    {
        /// <inheritdoc />
        public BlobMetadataDictionary()
        {
        }

        /// <inheritdoc />
        public BlobMetadataDictionary(string blobName, Dictionary<string, object> blobMeta)
            : base(null, blobName, blobMeta)
        {
        }

        /// <summary>
        /// Gets name of Blob
        /// </summary>
        public string BlobName => RowKey;
    }
}