// <copyright file="CrawlerAzureStorage.cs" company="DECTech.Tokyo">
// Copyright (c) DECTech.Tokyo. All rights reserved.
// </copyright>

namespace CrawlerLib.Azure
{
    using System;
    using System.Collections.Async;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Data;
    using global::Azure.Storage;
    using JetBrains.Annotations;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Auth;
    using Microsoft.WindowsAzure.Storage.Blob;
    using System.Linq;
    using CrawlerLib.Logger;
    using System;
    using System.Threading;

    [UsedImplicitly]
    public class CrawlerAzureStorage : ICrawlerStorage
    {
        private readonly DataStorage storage;
        private readonly IBlobSearcher searcher;

        public CrawlerAzureStorage(DataStorage storage, IBlobSearcher searcher)
        {
            this.storage = storage;
            this.searcher = searcher;
        }

        public async Task DumpPage(string uri, Stream content)
        {
            var container = await storage.GetBlobContainer("pages");

            var record = new CrawlRecord(uri)
            {
                Status = HttpStatusCode.OK.ToString()
            };


            var blob = container.GetBlockBlobReference(record.BlobName);
            await blob.UploadFromStreamAsync(content);

            await storage.InsertOrReplaceAsync(record);
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
            var blob = container.GetBlockBlobReference(DataStorage.EncodeString(uri));
            await blob.DownloadToStreamAsync(destination, cancellation);
        }

        public Task<IEnumerable<string>> GetReferers(string sessionId, string uri)
        {
            return Task.FromResult(storage.Query(new UriReferer(sessionId, uri, null)).AsEnumerable().Select(e => e.Referer));
        }

        public async Task<IAsyncEnumerable<string>> SearchText(string text)
        {
            var en = await searcher.SearchByText(text);
            return en.Select(DataStorage.DecodeString);
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