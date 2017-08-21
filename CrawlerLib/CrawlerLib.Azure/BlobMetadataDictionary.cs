// <copyright file="BlobMetadataDictionary.cs" company="DECTech.Tokyo">
// Copyright (c) DECTech.Tokyo. All rights reserved.
// </copyright>

namespace CrawlerLib.Azure
{
    using System;
    using System.Collections.Generic;
    using global::Azure.Storage;

    /// <inheritdoc />
    /// <summary>
    /// Set of metadata from Blob
    /// </summary>
    [Table("blobmetadata")]
    public class BlobMetadataDictionary : DictionaryTableEntity
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BlobMetadataDictionary"/> class.
        /// </summary>
        public BlobMetadataDictionary()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BlobMetadataDictionary"/> class.
        /// </summary>
        /// <param name="ownerId">Blob owner Id.</param>
        /// <param name="blobName">Name of blob.</param>
        /// <param name="blobMeta">Metadata dictionary.</param>
        public BlobMetadataDictionary(
            string ownerId,
            string blobName,
            Dictionary<string, object> blobMeta = null)
            : base(ownerId, Uri.EscapeDataString(blobName), blobMeta)
        {
        }

        /// <summary>
        /// Gets name of Blob
        /// </summary>
        public string BlobName => Uri.UnescapeDataString(RowKey);
    }
}