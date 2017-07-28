// <copyright file="MetadataString.cs" company="DECTech.Tokyo">
// Copyright (c) DECTech.Tokyo. All rights reserved.
// </copyright>

namespace CrawlerLib.Azure
{
    using System;
    using global::Azure.Storage;
    using Microsoft.WindowsAzure.Storage.Table;

    /// <inheritdoc />
    /// <summary>
    /// Single metadata item with string value
    /// </summary>
    [Table("metadata", PartitionKey = "const")]
    public class MetadataString : TableEntity
    {
        /// <inheritdoc />
        public MetadataString()
            : base(null, Guid.NewGuid().ToString())
        {
        }

        /// <summary>
        /// Gets or sets name of Blob containing this metadata.
        /// </summary>
        public string BlobName { get; set; }

        /// <summary>
        /// Gets or sets name of metadata field.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets metadata value.
        /// </summary>
        public string Value { get; set; }
    }
}