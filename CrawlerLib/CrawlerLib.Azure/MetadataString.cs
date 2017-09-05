// <copyright file="MetadataString.cs" company="DECTech.Tokyo">
// Copyright (c) DECTech.Tokyo. All rights reserved.
// </copyright>

namespace CrawlerLib.Azure
{
    using global::Azure.Storage;
    using JetBrains.Annotations;
    using Microsoft.WindowsAzure.Storage.Table;

    /// <inheritdoc />
    /// <summary>
    /// Single metadata item with string value
    /// </summary>
    [Table("metadata")]
    public class MetadataString : TableEntity
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MetadataString"/> class.
        /// </summary>
        public MetadataString()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MetadataString"/> class.
        /// </summary>
        /// <param name="ownerid">Blob owner Id.</param>
        /// <param name="blobname">Name of the blob.</param>
        /// <param name="metaname">Metadata name.</param>
        /// <param name="metavalue">Metadata value.</param>
        public MetadataString(string ownerid, string blobname, string metaname, string metavalue)
            : base(ownerid, Codec.HashString(blobname + metaname + metavalue))
        {
            BlobName = blobname;
            Name = metaname;
            Value = metavalue;
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

        /// <summary>
        /// Gets blob owner Id.
        /// </summary>
        [PartitionKey]
        public string OwnerId => PartitionKey;
    }
}