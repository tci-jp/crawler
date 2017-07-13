namespace CrawlerLib.Azure
{
    using System.IO;
    using System.Net;
    using System.Threading.Tasks;
    using global::Azure.Storage;
    using JetBrains.Annotations;

    [UsedImplicitly]
    public class CrawlerAzureStorage : ICrawlerStorage
    {
        private readonly DataStorage storage;

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