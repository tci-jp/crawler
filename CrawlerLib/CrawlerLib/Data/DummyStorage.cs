// <copyright file="DummyStorage.cs" company="DECTech.Tokyo">
// Copyright (c) DECTech.Tokyo. All rights reserved.
// </copyright>

namespace CrawlerLib.Data
{
    using System;
    using System.Collections.Async;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    /// <inheritdoc />
    /// <summary>
    /// In-memory implementation for Crawler Storage
    /// </summary>
    public class DummyStorage : ICrawlerStorage, IMetadataStorage, IDataSearch
    {
        private readonly ConcurrentDictionary<string, byte[]> dumpedPages =
            new ConcurrentDictionary<string, byte[]>();

        private readonly ConcurrentDictionary<string, SessionInfo> sessions =
            new ConcurrentDictionary<string, SessionInfo>();

        /// <inheritdoc />
        public Task AddPageReferer(string sessionId, string uri, string referer)
        {
            sessions[sessionId].Referers.GetOrAdd(uri, key => new ConcurrentBag<string>()).Add(referer);
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public Task<string> CreateSession(string ownerId, IEnumerable<string> rootUris, DateTime? cancellationTime)
        {
            var sess = new SessionInfo
                       {
                           Id = Guid.NewGuid().ToString(),
                           RootUris = new List<string>(rootUris),
                           Timestamp = DateTime.UtcNow
                       };

            sessions.TryAdd(sess.Id, sess);
            return Task.FromResult(sess.Id);
        }

        /// <inheritdoc />
        public async Task DumpUriContent(
            string ownerId,
            string sessionId,
            string uri,
            Stream stream,
            CancellationToken cancellation)
        {
            var mem = new MemoryStream();
            await stream.CopyToAsync(mem);
            dumpedPages.AddOrUpdate(uri, mem.ToArray(), (key, value) => value);
        }

        /// <inheritdoc/>
        public Task DumpUriMetadataAsync(
            string ownerId,
            string sessionId,
            string uri,
            IEnumerable<KeyValuePair<string, string>> metadata,
            CancellationToken cancellation)
        {
            // throw new NotImplementedException();
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public Task<bool> EnqueSessionUri(string sessionId, string uri)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public IAsyncEnumerable<string> GetAvailableMetadata()
        {
            return Enumerable.Empty<string>().ToAsyncEnumerable();
        }

        /// <inheritdoc />
        public IAsyncEnumerable<string> GetReferers(string sessionId, string uri)
        {
            return sessions[sessionId].Referers[uri].ToAsyncEnumerable();
        }

        /// <inheritdoc />
        public Task<IPage<ISessionInfo>> GetSessions(
            string ownerId,
            IEnumerable<string> sessionIds,
            int pageSize = 10,
            string requestId = null,
            CancellationToken cancellation = default(CancellationToken))
        {
            var sess = sessionIds?.Select(id => sessions[id]) ?? sessions.Values;
            return Task.FromResult<IPage<ISessionInfo>>(new Page<ISessionInfo>(sess, null));
        }

        /// <inheritdoc />
        public IAsyncEnumerable<IUriState> GetSessionUris(string sessionId)
        {
            return sessions[sessionId]
                .Referers
                .Keys
                .Select(k => (IUriState)new UriState { Uri = k, State = 200 })
                .ToAsyncEnumerable();
        }

        /// <inheritdoc />
        public Task<ISessionInfo> GetSingleSession(string ownerId, string sessionId)
        {
            return Task.FromResult<ISessionInfo>(sessions[sessionId]);
        }

        /// <inheritdoc />
        public Task<Stream> GetUriContent(
            string ownerId,
            string uri,
            CancellationToken cancellation = default(CancellationToken))
        {
            return Task.FromResult<Stream>(new MemoryStream(dumpedPages[uri]));
        }

        /// <inheritdoc />
        public IAsyncEnumerable<KeyValuePair<string, string>> GetUriMetadata(
            string ownerId,
            string uri,
            CancellationToken calncellation)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public IAsyncEnumerable<IParserParameters> RetreiveAllParserParametersAsync(string ownerId)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public Task<IParserParameters> RetreiveParserParametersAsync(string ownerId, string parserId)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public IAsyncEnumerable<string> SearchByMeta(
            IEnumerable<SearchCondition> query,
            CancellationToken cancellation)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public IAsyncEnumerable<string> SearchByText(string text, CancellationToken cancellation)
        {
            IEnumerable<string> GetEnumerable()
            {
                foreach (var page in dumpedPages)
                {
                    var str = Encoding.UTF8.GetString(page.Value);
                    if (str.IndexOf(text, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        yield return page.Key;
                    }
                }
            }

            return GetEnumerable().ToAsyncEnumerable();
        }

        /// <inheritdoc />
        public Task StorePageError(string ownerid, string sessionId, string uri, HttpStatusCode code)
        {
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public Task StoreParserParametersAsync(IParserParameters parserParameters)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public Task UpdateSessionCancellation(string ownerId, string sessionId, DateTime? cancellation)
        {
            sessions[sessionId].CancellationTime = cancellation;
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public Task UpdateSessionState(string ownerId, string sessionId, SessionState state)
        {
            sessions[sessionId].State = state;
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public Task UpdateSessionUri(string sessionId, string uri, int statusCode, string message = null)
        {
            throw new NotImplementedException();
        }

        private class SessionInfo : ISessionInfo
        {
            public DateTime? CancellationTime { get; set; }

            public string Id { get; set; }

            public string OwnerId { get; set; }

            public ConcurrentDictionary<string, ConcurrentBag<string>> Referers { get; } =
                new ConcurrentDictionary<string, ConcurrentBag<string>>();

            public IList<string> RootUris { get; set; } = new List<string>();

            public SessionState State { get; set; }

            public DateTime Timestamp { get; set; }
        }

        /// <inheritdoc />
        private class UriState : IUriState
        {
            /// <inheritdoc />
            public int State { get; set; }

            /// <inheritdoc />
            public string Uri { get; set; }
        }
    }
}