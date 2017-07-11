namespace CrawlerLib.Azure
{
    using System.IO;
    using System.Net;
    using System.Threading.Tasks;
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

    public class CrawlerAzureStorage : ICrawlerStorage
    {
        private DataStorage storage;

        public CrawlerAzureStorage(DataStorage storage)
        {
            this.storage = storage;
        }



        public async Task DumpPage(string uri, Stream content)
        {
            var container = await storage.GetBlobContainer("pages");
            var blob = container.GetBlockBlobReference(uri);
            await blob.UploadFromStreamAsync(content);

            await storage.InsertOrReplaceAsync(new CrawlRecord(uri)
            {
                Status = "OK"
            });
        }

        public async Task StorePageError(string uri, HttpStatusCode code)
        {
            await storage.InsertOrReplaceAsync(new CrawlRecord(uri)
            {
                Status = code.ToString()
            });
        }
    }
}