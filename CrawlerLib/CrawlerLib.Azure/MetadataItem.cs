// <copyright file="MetadataItem.cs" company="DECTech.Tokyo">
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
    /// Metadata item used to collect all used metadata field names.
    /// </summary>
    [Table("common", PartitionKey = "metadata")]
    public class MetadataItem : TableEntity
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MetadataItem"/> class.
        /// </summary>
        public MetadataItem()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MetadataItem"/> class.
        /// </summary>
        /// <param name="name">Name of metadata field.</param>
        public MetadataItem(string name)
            : base(null, Guid.NewGuid().ToString())
        {
            Name = name;
        }

        /// <summary>
        /// Gets or sets gets metadata field name.
        /// </summary>
        public string Name
        {
            get;
            [UsedImplicitly]
            set;
        }
    }
}