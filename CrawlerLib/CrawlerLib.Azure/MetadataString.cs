// <copyright file="MetadataString.cs" company="DECTech.Tokyo">
// Copyright (c) DECTech.Tokyo. All rights reserved.
// </copyright>

namespace CrawlerLib.Azure
{
    using System;
    using global::Azure.Storage;
    using JetBrains.Annotations;
    using Microsoft.WindowsAzure.Storage.Table;

    /// <inheritdoc />
    /// <summary>
    /// Single metadata item with string value
    /// </summary>
    [Table("metadata", PartitionKey = "const")]
    public class MetadataString : TableEntity
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MetadataString"/> class.
        /// </summary>
        public MetadataString()
            : base(null, Guid.NewGuid().ToString())
        {
        }

        /// <summary>
        /// Gets or sets name of Blob containing this metadata.
        /// </summary>
        public string BlobName
        {
            [UsedImplicitly]
            get;
            set;
        }

        /// <summary>
        /// Gets or sets name of metadata field.
        /// </summary>
        public string Name
        {
            [UsedImplicitly]
            get;
            set;
        }

        /// <summary>
        /// Gets or sets metadata value.
        /// </summary>
        public string Value
        {
            [UsedImplicitly]
            get;
            set;
        }
    }
}