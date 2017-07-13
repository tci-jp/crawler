namespace CrawlerLib.Azure
{
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using System.Threading.Tasks;
    using Data;
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

        public Task<string> CreateSession(IEnumerable<string> rootUris)
        {
            throw new System.NotImplementedException();
        }

        public Task<IEnumerable<SessionInfo>> GetAllSessions()
        {
            throw new System.NotImplementedException();
        }

        public Task AddPageReferer(string sessionId, string uri, string referer)
        {
            throw new System.NotImplementedException();
        }

        public Task StorePageError(string sessionId, string uri, HttpStatusCode code)
        {
            throw new System.NotImplementedException();
        }

        public Task<IEnumerable<string>> GetSessionUris(string sessionId)
        {
            throw new System.NotImplementedException();
        }

        public Task<Stream> GetUriContet(string uri)
        {
            throw new System.NotImplementedException();
        }

        public Task<IEnumerable<string>> GetReferers(string sessionId, string uri)
        {
            throw new System.NotImplementedException();
        }

        public Task<IEnumerable<string>> SearchText(string text)
        {
            throw new System.NotImplementedException();
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