// <copyright file="MetadataIndexItem.cs" company="DECTech.Tokyo">
// Copyright (c) DECTech.Tokyo. All rights reserved.
// </copyright>

namespace CrawlerLib.Azure
{
    using Microsoft.Azure.Search.Models;
    using Newtonsoft.Json;

    /// <summary>
    /// POCO for Metadata search index
    /// </summary>
    [SerializePropertyNamesAsCamelCase]
    public class MetadataIndexItem
    {
        /// <summary>
        /// Gets or sets blob name
        /// </summary>
        [JsonProperty("BlobName")]
        public string BlobName { get; set; }

        /// <summary>
        /// Gets or sets metadata field name
        /// </summary>
        [JsonProperty("Name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets metadata value name
        /// </summary>
        [JsonProperty("Value")]
        public string Value { get; set; }
    }
}