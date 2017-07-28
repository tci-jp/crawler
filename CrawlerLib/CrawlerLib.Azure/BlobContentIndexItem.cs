// <copyright file="BlobContentIndexItem.cs" company="DECTech.Tokyo">
// Copyright (c) DECTech.Tokyo. All rights reserved.
// </copyright>

namespace CrawlerLib.Azure
{
    using System.ComponentModel.DataAnnotations;
    using Microsoft.Azure.Search.Models;
    using Newtonsoft.Json;

    /// <summary>
    /// POCO for Plain text search index
    /// </summary>
    [SerializePropertyNamesAsCamelCase]
    public class BlobContentIndexItem
    {
        /// <summary>
        /// Gets or sets blob name
        /// </summary>
        [Key]
        [JsonProperty("metadata_storage_name")]
        public string MetadataStorageName { get; set; }
    }
}