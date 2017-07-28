namespace CrawlerLib.Azure
{
    using System;
    using Microsoft.Azure.Search;
    using Microsoft.Azure.Search.Models;
    using Microsoft.Spatial;
    using Newtonsoft.Json;

    [SerializePropertyNamesAsCamelCase]
    public class Blob
    {
        [System.ComponentModel.DataAnnotations.Key]
        [JsonProperty("metadata_storage_name")]
        public string MetadataStorageName { get; set; }
    }
}