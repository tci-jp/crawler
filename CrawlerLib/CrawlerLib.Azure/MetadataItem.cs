// <copyright file="MetadataItem.cs" company="DECTech.Tokyo">
// Copyright (c) DECTech.Tokyo. All rights reserved.
// </copyright>

namespace CrawlerLib.Azure
{
    using System;
    using global::Azure.Storage;
    using Microsoft.WindowsAzure.Storage.Table;

    /// <inheritdoc />
    /// <summary>
    /// Metadata item used to collect all used metadata field names.
    /// </summary>
    [Table("common", PartitionKey = "metadata")]
    public class MetadataItem : TableEntity
    {
        /// <inheritdoc />
        public MetadataItem()
        {
        }

        /// <inheritdoc />
        public MetadataItem(string name)
            : base(null, Guid.NewGuid().ToString())
        {
            Name = name;
        }

        /// <summary>
        /// Gets or sets gets metadata field name.
        /// </summary>
        public string Name { get; set; }
    }
}