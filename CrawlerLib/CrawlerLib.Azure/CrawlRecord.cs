namespace CrawlerLib.Azure
{
    using global::Azure.Storage;
    using Microsoft.WindowsAzure.Storage.Table;

    [Table("common", PartitionKey = "url")]
    public class CrawlRecord : TableEntity
    {
        public CrawlRecord(string url)
            : base(null, url)
        {
        }

        public string Url => RowKey;

        public string Status { get; set; }
    }
}