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

    /// <summary>
    /// In-memory implementation of Crawler Storage
    /// </summary>
    public class DummyStorage : ICrawlerStorage
    {
        private readonly ConcurrentDictionary<string, byte[]> dumpedPages = new ConcurrentDictionary<string, byte[]>();

        private readonly ConcurrentDictionary<string, SessionInfo> sessions = new ConcurrentDictionary<string, SessionInfo>();

        /// <inheritdoc />
        public async Task DumpPage(string uri, Stream stream)
        {
            var mem = new MemoryStream();
            await stream.CopyToAsync(mem);
            dumpedPages.AddOrUpdate(uri, mem.ToArray(), (key, value) => value);
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
        public Task<IEnumerable<ISessionInfo>> GetAllSessions()
        {
            return Task.FromResult(sessions.Cast<ISessionInfo>());
        }

        /// <inheritdoc />
        public Task AddPageReferer(string sessionId, string uri, string referer)
        {
            sessions[sessionId].Referers.GetOrAdd(uri, key => new ConcurrentBag<string>()).Add(referer);
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public Task StorePageError(string sessionId, string uri, HttpStatusCode code)
        {
            sessions[sessionId].Codes[uri] = code.ToString();
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public Task<IEnumerable<string>> GetSessionUris(string sessionId)
        {
            return Task.FromResult<IEnumerable<string>>(sessions[sessionId].Codes.Keys);
        }

        /// <inheritdoc />
        public async Task GetUriContet(string uri, Stream destination, CancellationToken cancellation)
        {
            await destination.WriteAsync(dumpedPages[uri], 0, dumpedPages[uri].Length, cancellation);
        }

        /// <inheritdoc />
        public Task<IEnumerable<string>> GetReferers(string sessionId, string uri)
        {
            return Task.FromResult<IEnumerable<string>>(sessions[sessionId].Referers[uri]);
        }

        /// <inheritdoc />
        public Task<IAsyncEnumerable<string>> SearchText(string text)
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

        private class SessionInfo : ISessionInfo
        {
            public ConcurrentDictionary<string, ConcurrentBag<string>> Referers { get; } =
                new ConcurrentDictionary<string, ConcurrentBag<string>>();

            public ConcurrentDictionary<string, string> Codes { get; } =
                new ConcurrentDictionary<string, string>();

            public string Id { get; set; }

            public DateTime Timestamp { get; set; }

            public IList<string> RootUris { get; set; } = new List<string>();
        }
    }
}