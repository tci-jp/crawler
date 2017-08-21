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
    public class DummyStorage : ICrawlerStorage
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
        public Task<string> CreateSession(string ownerId, IEnumerable<string> rootUris)
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
            CancellationToken cancellation,
            IEnumerable<KeyValuePair<string, string>> meta = null)
        {
            var mem = new MemoryStream();
            await stream.CopyToAsync(mem);
            dumpedPages.AddOrUpdate(uri, mem.ToArray(), (key, value) => value);
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
            return sessions[sessionId].Referers.Keys.Select(k => (IUriState)new UriState
                                                                            {
                                                                                Uri = k,
                                                                                State = 200
                                                                            })
                                      .ToAsyncEnumerable();
        }

        /// <inheritdoc />
        public async Task GetUriContet(string ownerId, string uri, Stream destination, CancellationToken cancellation)
        {
            await destination.WriteAsync(dumpedPages[uri], 0, dumpedPages[uri].Length, cancellation);
        }

        /// <inheritdoc />
        public IAsyncEnumerable<KeyValuePair<string, string>> GetUriMetadata(
            string ownerId,
            string uri,
            CancellationToken calncellation)
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

        private class SessionInfo : ISessionInfo
        {
            public string Id { get; set; }

            public string OwnerId { get; set; }

            public ConcurrentDictionary<string, ConcurrentBag<string>> Referers { get; } =
                new ConcurrentDictionary<string, ConcurrentBag<string>>();

            public IList<string> RootUris { get; set; } = new List<string>();

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