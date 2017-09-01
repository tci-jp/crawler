// <copyright file="AzureCrawlerStorage.cs" company="DECTech.Tokyo">
// Copyright (c) DECTech.Tokyo. All rights reserved.
// </copyright>

namespace CrawlerLib.Azure
{
    using System;
    using System.Collections.Async;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Threading.Tasks;
    using Data;
    using global::Azure.Storage;
    using JetBrains.Annotations;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Blob;
    using Microsoft.WindowsAzure.Storage.Table;
    using Newtonsoft.Json;

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
        private readonly IDataStorage storage;

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureCrawlerStorage" /> class.
        /// </summary>
        /// <param name="storage">Azure Storage helper class.</param>
        /// <param name="searcher">Blob searcher.</param>
        public AzureCrawlerStorage(IDataStorage storage, IBlobSearcher searcher)
        {
            this.storage = storage;
            this.searcher = searcher;
        }

        /// <inheritdoc />
        public async Task AddPageReferer(string sessionId, string uri, string referer)
        {
            await storage.InsertOrReplaceAsync(new UriReferer(sessionId, uri, referer));
        }

        /// <inheritdoc />
        public async Task<string> CreateSession(string ownerId, IEnumerable<string> rootUris)
        {
            var uris = rootUris.ToList();
            var session = new SessionInfo(ownerId, uris)
            {
                State = SessionState.InProcess
            };
            await storage.InsertOrReplaceAsync(session);
            return session.Id;
        }

        /// <inheritdoc />
        public async Task DumpUriContent(
            string ownerId,
            string sessionId,
            string uri,
            Stream content,
            CancellationToken cancellation,
            IEnumerable<KeyValuePair<string, string>> metadata = null)
        {
            var container = await storage.GetBlobContainerAsync("pages");

            var record = new CrawlRecord(ownerId, uri)
            {
                Status = HttpStatusCode.OK.ToString()
            };

            var blob = container.GetBlockBlobReference(record.BlobName);
            await blob.UploadFromStreamAsync(
                content,
                new AccessCondition(),
                new BlobRequestOptions(),
                new OperationContext(),
                cancellation);

            await storage
                .QueryAsync<MetadataString>(m => (m.PartitionKey == ownerId) && (m.BlobName == record.BlobName))
                .ForEachAsync(
                    async meta => { await storage.DeleteAsync(meta).ConfigureAwait(false); },
                    cancellation);

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
                    await storage.InsertAsync(
                        new MetadataString(ownerId, record.BlobName, Codec.EncodeString(pair.Key), pair.Value));
                    var metaname = pairkey;
                    for (var index = 1; blobMeta.ContainsKey(metaname); index++)
                    {
                        metaname = pairkey + "_" + index;
                    }

                    blobMeta[metaname] = pair.Value;
                }

                await storage.InsertOrReplaceAsync(new BlobMetadataDictionary(
                                                       ownerId,
                                                       record.BlobName,
                                                       blobMeta));
            }

            await storage.InsertOrReplaceAsync(new SessionUri(sessionId, uri, 200));
            await storage.InsertOrReplaceAsync(record);
        }

        /// <inheritdoc />
        public async Task EnqueSessionUri(string sessionId, string uri)
        {
            await storage.InsertAsync(new SessionUri(sessionId, uri, 0));
        }

        /// <inheritdoc />
        public IAsyncEnumerable<string> GetAvailableMetadata()
        {
            return storage.QueryAsync<MetadataItem>().Select(m => m.Name);
        }

        /// <inheritdoc />
        public IAsyncEnumerable<string> GetReferers(string sessionId, string uri)
        {
            return storage.QueryAsync(new UriReferer(sessionId, uri, null))
                          .Select(e => e.Referer);
        }

        /// <inheritdoc />
        public async Task<IPage<ISessionInfo>> GetSessions(
            string ownerId,
            IEnumerable<string> sessionIds,
            int pageSize = 10,
            string requestId = null,
            CancellationToken cancellation = default(CancellationToken))
        {
            var filter = new StringBuilder();
            filter.Append("(").Append(TableQuery.GenerateFilterCondition("PartitionKey", "eq", ownerId)).Append(")");
            if (sessionIds != null)
            {
                var enumerable = sessionIds.Select(s => $"({TableQuery.GenerateFilterCondition("RowKey", "eq", s)})");
                var infilter = string.Join(" or ", enumerable);
                if (!string.IsNullOrWhiteSpace(infilter))
                {
                    filter.Append(" and (");
                    filter.Append(infilter);
                    filter.Append(")");
                }
            }

            var query = new TableQuery<SessionInfo>
            {
                FilterString = filter.ToString(),
                TakeCount = pageSize
            };

            var token = requestId == null ? null : FromBase64<TableContinuationToken>(requestId);
            var segment = await storage.QuerySegmentedAsync(query, token, cancellation);

            var newRequestId = segment?.ContinuationToken == null ? null : ToBase64(segment.ContinuationToken);
            return new Page<ISessionInfo>(segment ?? Enumerable.Empty<ISessionInfo>(), newRequestId);
        }

        /// <inheritdoc />
        public IAsyncEnumerable<IUriState> GetSessionUris(string sessionId)
        {
            return storage.QueryAsync<SessionUri>(s => s.SessionId == sessionId).Cast<IUriState>();
        }

        /// <inheritdoc />
        public async Task<Stream> GetUriContent(string ownerId, string uri, CancellationToken cancellation)
        {
            var rec = new CrawlRecord(ownerId, uri);
            var container = await storage.GetBlobContainerAsync("pages");
            var blob = container.GetBlockBlobReference(rec.BlobName);
            return await blob.OpenReadAsync(
                new AccessCondition(),
                new BlobRequestOptions(),
                new OperationContext(),
                cancellation);
        }

        /// <inheritdoc />
        public IAsyncEnumerable<KeyValuePair<string, string>> GetUriMetadata(
            string ownerId,
            string uri,
            CancellationToken cancellation)
        {
            var rec = new CrawlRecord(ownerId, uri);
            return storage
                .QueryAsync<MetadataString>(m => (m.OwnerId == ownerId) && (m.BlobName == rec.BlobName), cancellation)
                .Select(m => new KeyValuePair<string, string>(Codec.DecodeString(m.Name), m.Value));
        }

        /// <inheritdoc />
        public IAsyncEnumerable<string> SearchByMeta(
            IEnumerable<SearchCondition> query,
            CancellationToken cancellation)
        {
            return searcher.SearchByMeta(
                query.Select(c => new SearchCondition
                {
                    Name = EscapeMetadataName(c.Name),
                    Op = c.Op,
                    Value = c.Value
                }),
                cancellation).Select(Codec.DecodeString);
        }

        /// <inheritdoc />
        public IAsyncEnumerable<string> SearchByText(string text, CancellationToken cancellation)
        {
            return searcher.SearchByText(text, cancellation).Select(Codec.DecodeString);
        }

        /// <inheritdoc />
        [UsedImplicitly]
        public async Task StorePageError(string ownerid, string sessionId, string uri, HttpStatusCode code)
        {
            await storage.InsertOrReplaceAsync(new CrawlRecord(ownerid, uri)
            {
                Status = code.ToString()
            });

            await storage.InsertOrReplaceAsync(new SessionUri(sessionId, uri, (int)code));
        }

        /// <inheritdoc />
        public async Task UpdateSessionState(string ownerId, string sessionId, SessionState state)
        {
            var session = await storage.RetreiveAsync(new SessionInfo(ownerId, sessionId));
            if (session == null)
            {
                throw new InvalidOperationException("Session does not exist: " + sessionId);
            }

            session.State = state;
            await storage.InsertOrReplaceAsync(session);
        }

        /// <inheritdoc />
        public async Task UpdateSessionUri(string sessionId, string uri, int statusCode)
        {
            await storage.InsertOrReplaceAsync(new SessionUri(sessionId, uri, statusCode));
        }

        private static string EscapeMetadataName(string key)
        {
            const string http = "http://";
            var result = key.StartsWith(http) ? key.Substring(http.Length) : key;
            return WrongCharRegex.Replace(result, "_").Trim('_'); // O_o
        }

        private static TEntity FromBase64<TEntity>(string str)
        {
            return JsonConvert.DeserializeObject<TEntity>(Encoding.UTF8.GetString(Convert.FromBase64String(str)));
        }

        private static string ToBase64(object obj)
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(obj)));
        }
    }
}