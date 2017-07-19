namespace CrawlerLib.Azure
{
    using System;
    using Microsoft.Azure.Search;
    using Microsoft.Azure.Search.Models;
    using Microsoft.Spatial;
    using Newtonsoft.Json;

    [SerializePropertyNamesAsCamelCase]
    public partial class Blob
    {
        [System.ComponentModel.DataAnnotations.Key]
        public string BlobUrl { get; set; }

        [IsSearchable]
        public string Html { get; set; }
    }
}