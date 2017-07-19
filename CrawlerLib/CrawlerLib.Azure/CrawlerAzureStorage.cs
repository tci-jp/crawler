namespace CrawlerLib.Azure
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Threading;
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
                Status = HttpStatusCode.OK.ToString()
            });
        }

        public async Task<string> CreateSession(IEnumerable<string> rootUris)
        {
            var session = new SessionInfo(rootUris);
            await storage.InsertOrReplaceAsync(session);
            return session.Id;
        }

        public Task<IEnumerable<ISessionInfo>> GetAllSessions()
        {
            return Task.FromResult(storage.Query<SessionInfo>().AsEnumerable().Cast<ISessionInfo>());
        }

        public async Task AddPageReferer(string sessionId, string uri, string referer)
        {
            await storage.InsertOrReplaceAsync(new UriReferer(sessionId, uri, referer));
            await storage.InsertOrReplaceAsync(new SessionUri(sessionId, uri));
        }

        public async Task StorePageError(string sessionId, string uri, HttpStatusCode code)
        {
            await storage.InsertOrReplaceAsync(new CrawlRecord(uri)
            {
                Status = code.ToString()
            });
        }

        public Task<IEnumerable<string>> GetSessionUris(string sessionId)
        {
            return Task.FromResult(storage.Query<SessionUri>().AsEnumerable().Select(u => u.Uri));
        }

        public async Task GetUriContet(string uri, Stream destination, CancellationToken cancellation)
        {
            var container = await storage.GetBlobContainer("pages");
            var blob = container.GetBlockBlobReference(uri);
            await blob.DownloadToStreamAsync(destination, cancellation);
        }

        public Task<IEnumerable<string>> GetReferers(string sessionId, string uri)
        {
            return Task.FromResult(storage.Query(new UriReferer(sessionId, uri, null)).AsEnumerable().Select(e => e.Referer));
        }

        public Task<IEnumerable<string>> SearchText(string text)
        {

        }

        [UsedImplicitly]
        public async Task StorePageError(string uri, HttpStatusCode code)
        {
            await storage.InsertOrReplaceAsync(new CrawlRecord(uri)
            {
                Status = code.ToString()
            });
        }
    }
}