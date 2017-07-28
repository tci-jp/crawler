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
        private readonly ConcurrentDictionary<string, string> codes =
            new ConcurrentDictionary<string, string>();

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
        public Task<string> CreateSession(IEnumerable<string> rootUris)
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
        public async Task DumpPage(
            string uri,
            Stream stream,
            CancellationToken cancellation,
            IEnumerable<KeyValuePair<string, string>> meta)
        {
            var mem = new MemoryStream();
            await stream.CopyToAsync(mem);
            dumpedPages.AddOrUpdate(uri, mem.ToArray(), (key, value) => value);
        }

        /// <inheritdoc />
        public Task<IEnumerable<ISessionInfo>> GetAllSessions()
        {
            return Task.FromResult(sessions.Cast<ISessionInfo>());
        }

        /// <inheritdoc />
        public Task<IEnumerable<string>> GetAvailableMetadata()
        {
            return Task.FromResult(Enumerable.Empty<string>());
        }

        /// <inheritdoc />
        public Task<IEnumerable<string>> GetReferers(string sessionId, string uri)
        {
            return Task.FromResult<IEnumerable<string>>(sessions[sessionId].Referers[uri]);
        }

        /// <inheritdoc />
        public Task<IEnumerable<string>> GetSessionUris(string sessionId)
        {
            return Task.FromResult<IEnumerable<string>>(sessions[sessionId].Uris);
        }

        /// <inheritdoc />
        public async Task GetUriContet(string uri, Stream destination, CancellationToken cancellation)
        {
            await destination.WriteAsync(dumpedPages[uri], 0, dumpedPages[uri].Length, cancellation);
        }

        /// <inheritdoc />
        public Task<IAsyncEnumerable<string>> SearchByMeta(
            IEnumerable<SearchCondition> query,
            CancellationToken cancellation)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public Task<IAsyncEnumerable<string>> SearchByText(string text, CancellationToken cancellation)
        {
            IEnumerable<string> GetEnumerable()
            {
                foreach (var page in dumpedPages)
                {
                    var str = Encoding.UTF8.GetString(page.Value);
                    if (str.IndexOf(text, StringComparison.InvariantCultureIgnoreCase) >= 0)
                    {
                        yield return page.Key;
                    }
                }
            }

            return Task.FromResult(GetEnumerable().ToAsyncEnumerable());
        }

        /// <inheritdoc />
        public Task StorePageError(string uri, HttpStatusCode code)
        {
            codes[uri] = code.ToString();
            return Task.CompletedTask;
        }

        private class SessionInfo : ISessionInfo
        {
            public ConcurrentDictionary<string, ConcurrentBag<string>> Referers { get; } =
                new ConcurrentDictionary<string, ConcurrentBag<string>>();

            public ConcurrentBag<string> Uris { get; } = new ConcurrentBag<string>();

            public string Id { get; set; }

            public DateTime Timestamp { get; set; }

            public IList<string> RootUris { get; set; } = new List<string>();
        }
    }
}