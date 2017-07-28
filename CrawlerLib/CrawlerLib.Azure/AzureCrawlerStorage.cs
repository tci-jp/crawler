// <copyright file="AzureCrawlerStorage.cs" company="DECTech.Tokyo">
// Copyright (c) DECTech.Tokyo. All rights reserved.
// </copyright>

namespace CrawlerLib.Azure
{
    using System.Collections.Async;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Threading.Tasks;
    using Data;
    using global::Azure.Storage;
    using JetBrains.Annotations;

    /// <inheritdoc />
    /// <summary>
    /// Crawler storage using Azure Storage.
    /// </summary>
    [UsedImplicitly]
    public class AzureCrawlerStorage : ICrawlerStorage
    {
        private static readonly Regex WrongCharRegex = new Regex("[^\\w\\d_]+");
        private readonly ConcurrentDictionary<string, bool> metadataSet = new ConcurrentDictionary<string, bool>();
        private readonly IBlobSearcher searcher;
        private readonly DataStorage storage;

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureCrawlerStorage" /> class.
        /// </summary>
        /// <param name="storage">Azure Storage helper class.</param>
        /// <param name="searcher">Blob searcher.</param>
        public AzureCrawlerStorage(DataStorage storage, IBlobSearcher searcher)
        {
            this.storage = storage;
            this.searcher = searcher;
        }

        /// <inheritdoc />
        public async Task AddPageReferer(string sessionId, string uri, string referer)
        {
            await storage.InsertOrReplaceAsync(new UriReferer(sessionId, uri, referer));
            await storage.InsertOrReplaceAsync(new SessionUri(sessionId, uri));
        }

        /// <inheritdoc />
        public async Task<string> CreateSession(IEnumerable<string> rootUris)
        {
            var session = new SessionInfo(rootUris);
            await storage.InsertOrReplaceAsync(session);
            return session.Id;
        }

        /// <inheritdoc />
        public async Task DumpPage(
            string uri,
            Stream content,
            CancellationToken cancellation,
            IEnumerable<KeyValuePair<string, string>> metadata = null)
        {
            var container = await storage.GetBlobContainer("pages");

            var record = new CrawlRecord(uri)
                         {
                             Status = HttpStatusCode.OK.ToString()
                         };

            var blob = container.GetBlockBlobReference(record.BlobName);
            await blob.UploadFromStreamAsync(content, cancellation);

            if (metadata != null)
            {
                var blobMeta = new Dictionary<string, object>();
                foreach (var pair in metadata)
                {
                    var added = metadataSet.AddOrUpdate(pair.Key, true, (k, v) => false);
                    if (added)
                    {
                        await storage.InsertOrReplaceAsync(new MetadataItem(pair.Key));
                    }

                    var pairkey = EscapeMetadataName(pair.Key);
                    await storage.InsertAsync(new MetadataString
                                              {
                                                  BlobName = record.BlobName,
                                                  Name = pairkey,
                                                  Value = pair.Value
                                              });
                    var metaname = pairkey;
                    for (var index = 1; blobMeta.ContainsKey(metaname); index++)
                    {
                        metaname = pairkey + "_" + index;
                    }

                    blobMeta[metaname] = pair.Value;
                }

                await storage.InsertOrReplaceAsync(new BlobMetadataDictionary(record.BlobName, blobMeta));
            }

            await storage.InsertOrReplaceAsync(record);
        }

        /// <inheritdoc />
        public Task<IEnumerable<ISessionInfo>> GetAllSessions()
        {
            return Task.FromResult(storage.Query<SessionInfo>().AsEnumerable().Cast<ISessionInfo>());
        }

        /// <inheritdoc />
        public Task<IEnumerable<string>> GetAvailableMetadata()
        {
            return Task.FromResult(storage.Query<MetadataItem>().Select(m => m.Name).AsEnumerable());
        }

        /// <inheritdoc />
        public Task<IEnumerable<string>> GetReferers(string sessionId, string uri)
        {
            return Task.FromResult(storage.Query(new UriReferer(sessionId, uri, null)).AsEnumerable()
                                          .Select(e => e.Referer));
        }

        /// <inheritdoc />
        public Task<IEnumerable<string>> GetSessionUris(string sessionId)
        {
            return Task.FromResult(storage.Query<SessionUri>().AsEnumerable().Select(u => u.Uri));
        }

        /// <inheritdoc />
        public async Task GetUriContet(string uri, Stream destination, CancellationToken cancellation)
        {
            var container = await storage.GetBlobContainer("pages");
            var blob = container.GetBlockBlobReference(DataStorage.EncodeString(uri));
            await blob.DownloadToStreamAsync(destination, cancellation);
        }

        /// <inheritdoc />
        public async Task<IAsyncEnumerable<string>> SearchByMeta(
            IEnumerable<SearchCondition> query,
            CancellationToken cancellation)
        {
            var en = await searcher.SearchByMeta(
                         query.Select(c => new SearchCondition
                                           {
                                               Name = EscapeMetadataName(c.Name),
                                               Op = c.Op,
                                               Value = c.Value
                                           }),
                         cancellation);
            return en.Select(DataStorage.DecodeString);
        }

        /// <inheritdoc />
        public async Task<IAsyncEnumerable<string>> SearchByText(string text, CancellationToken cancellation)
        {
            var en = await searcher.SearchByText(text, cancellation);
            return en.Select(DataStorage.DecodeString);
        }

        /// <inheritdoc />
        [UsedImplicitly]
        public async Task StorePageError(string uri, HttpStatusCode code)
        {
            await storage.InsertOrReplaceAsync(new CrawlRecord(uri)
                                               {
                                                   Status = code.ToString()
                                               });
        }

        private static string EscapeMetadataName(string key)
        {
            const string http = "http://";
            var result = key.StartsWith(http) ? key.Substring(http.Length) : key;
            return WrongCharRegex.Replace(result, "_").Trim('_'); // O_o
        }
    }
}