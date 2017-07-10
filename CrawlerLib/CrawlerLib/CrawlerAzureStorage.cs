namespace CrawlerLib
{
    using System.IO;
    using System.Threading.Tasks;

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
        }
    }
}