// <copyright file="Crawler.cs" company="DECTech.Tokyo">
// Copyright (c) DECTech.Tokyo. All rights reserved.
// </copyright>

namespace CrawlerLib
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Data;
    using HtmlAgilityPack;
    using JetBrains.Annotations;
    using Logger;

    /// <inheritdoc />
    /// <summary>
    /// Crawls, parses and indexes web pages.
    /// </summary>
    public sealed class Crawler : ICrawler
    {
        private readonly HttpClient client;

        private readonly Configuration config;

        private readonly ILinkParser linkParser = new LinkParser();

        private readonly ConcurrentDictionary<Uri, Task<Robots>> robots;
        private readonly ICrawlerStorage storage;

        private readonly SemaphoreSlim totalRequestsSemaphore;
        private readonly HashSet<string> visited;
        private readonly object visitedLock = new object();

        /// <summary>
        /// Initializes a new instance of the <see cref="Crawler" /> class.
        /// </summary>
        /// <param name="conf">Configuration for crawler.</param>
        public Crawler(Configuration conf = null)
        {
            // EncodingRedirector.RegisterEncodings();
            config = new Configuration(conf);

            client = new HttpClient
            {
                Timeout = config.RequestTimeout
            };

            client.DefaultRequestHeaders.UserAgent.ParseAdd(config.UserAgent);
            storage = config.Storage;

            robots = new ConcurrentDictionary<Uri, Task<Robots>>();
            visited = new HashSet<string>();

            totalRequestsSemaphore = new SemaphoreSlim(config.NumberOfSimulataneousRequests);
        }

        /// <summary>
        /// Called when crawler parsed and dumped new page
        /// </summary>
        [UsedImplicitly]
        public event Action<Crawler, string> UriCrawled;

        /// <inheritdoc />
        public void Dispose()
        {
            client?.Dispose();
        }

        /// <summary>
        /// Starts crawling by single URI
        /// </summary>
        /// <param name="ownerId">Session onwer Id.</param>
        /// <param name="uri">URI to crawl.</param>
        /// <returns>Session Id. A <see cref="Task" /> representing the asynchronous operation.</returns>
        [UsedImplicitly]
        public Task<string> Incite(string ownerId, Uri uri)
        {
            return Incite(ownerId, new[] { uri });
        }

        /// <summary>
        /// Starts crawling by collection of URIs
        /// </summary>
        /// <param name="ownerId">Session owner id.</param>
        /// <param name="uris">URIs to crawl.</param>
        /// <returns>Session Id. A <see cref="Task" /> representing the asynchronous operation.</returns>
        [UsedImplicitly]
        public async Task<string> Incite(string ownerId, IEnumerable<Uri> uris)
        {
            var urisList = new List<Uri>(uris);
            if (urisList.Count == 0)
            {
                return null;
            }

            var sessionid = await storage.CreateSession(ownerId, urisList.Select(u => u.ToString()));
            foreach (var uri in urisList)
            {
                var newjob = new ParserJob
                {
                    OwnerId = ownerId,
                    SessionId = sessionid,
                    Uri = uri,
                    Depth = config.Depth,
                    HostDepth = config.HostDepth
                };

                await EnqueueAsync(newjob);
            }

            await config.Queue.WaitForSession(sessionid, config.CancellationToken);
            return sessionid;
        }

        /// <summary>
        /// Starts crawling by collection of URIs
        /// </summary>
        /// <param name="ownerId">Session owner id.</param>
        /// <param name="uris">URIs to crawl.</param>
        /// <returns>Session Id. A <see cref="Task" /> representing the asynchronous operation.</returns>
        [UsedImplicitly]
        public async Task<string> InciteStart(string ownerId, IEnumerable<Uri> uris)
        {
            var urisList = new List<Uri>(uris);
            if (urisList.Count == 0)
            {
                return null;
            }

            var sessionid = await storage.CreateSession(ownerId, urisList.Select(u => u.ToString()));
            try
            {
                foreach (var uri in urisList)
                {
                    var newjob = new ParserJob
                    {
                        OwnerId = ownerId,
                        SessionId = sessionid,
                        Uri = uri,
                        Depth = config.Depth,
                        HostDepth = config.HostDepth
                    };
                    await EnqueueAsync(newjob);
                }
            }
            catch (Exception)
            {
                await storage.UpdateSessionState(ownerId, sessionid, SessionState.Error);
                throw;
            }

            return sessionid;
        }

        /// <summary>
        /// Run workers to parse queue.
        /// </summary>
        /// <param name="workers">Number of workers</param>
        public void RunParserWorkers(int workers)
        {
            for (var worker = 0; worker < workers; worker++)
            {
                var task = Task.Run(
                    async () => { await InternalRunParsersJobs(); },
                    config.CancellationToken);
            }
        }

        private static(bool nofollow, bool noindex) CheckNofollowNoindex(HtmlDocument html)
        {
            var nofollow = false;
            var noindex = false;
            foreach (var meta in html.DocumentNode.SelectNodes("//meta[name='robots']")?
                                     .Select(m => m.Attributes["content"].Value) ??
                                 new string[0])
            {
                if (meta.IndexOf("NOFOLLOW", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    nofollow = true;
                }

                if (meta.IndexOf("NOINDEX", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    noindex = true;
                }

                if (nofollow && noindex)
                {
                    break;
                }
            }

            return (nofollow, noindex);
        }

        private async Task AddUrl(IParserJob parent, Uri newUri)
        {
            if (parent.Depth <= 0)
            {
                return;
            }

            var newjob = new ParserJob
            {
                OwnerId = parent.OwnerId,
                SessionId = parent.SessionId,
                Uri = newUri,
                Depth = parent.Depth - 1,
                HostDepth = parent.HostDepth,
                Referrer = parent.Uri
            };

            if (parent.Host != newjob.Host)
            {
                if (newjob.HostDepth <= 0)
                {
                    return;
                }

                newjob.HostDepth--;
            }

            await config.Storage.AddPageReferer(parent.SessionId, newjob.Uri.ToString(), newjob.Referrer.ToString());

            lock (visitedLock)
            {
                if (!visited.Add(newjob.Uri.ToString()))
                {
                    return;
                }
            }

            var robotstxt = await GetRobotsTxt(newjob.Host);
            if (robotstxt?.IsPathAllowed(newjob.Uri.PathAndQuery) == false)
            {
                return;
            }

            await config.Storage.EnqueSessionUri(parent.SessionId, newjob.Uri.ToString());

            await config.Queue.EnqueueAsync(newjob, config.CancellationToken);
        }

        private async Task EnqueueAsync(IParserJob newjob)
        {
            lock (visitedLock)
            {
                visited.Add(newjob.Uri.ToString());
            }

            await config.Queue.EnqueueAsync(newjob, config.CancellationToken);
        }

        private IEnumerable<KeyValuePair<string, string>> ExtractMetadata(HtmlDocument html)
        {
            return config.MetadataExtractors.SelectMany(ex => ex.ExtractMetadata(html));
        }

        private Task<Robots> GetRobotsTxt(Uri host)
        {
            return robots.GetOrAdd(
                host,
                async roburi =>
                {
                    try
                    {
                        var robotstxt = await client.GetStringAsync(roburi + "/robots.txt");
                        return new Robots(config.UserAgent, robotstxt);
                    }
                    catch (HttpRequestException)
                    {
                        return null;
                    }
                });
        }

        private async Task InternalIncite(IParserJob job)
        {
            try
            {
                Exception lastException = null;
                var lastCode = HttpStatusCode.OK;

                for (var trycount = 0; trycount < config.RetriesNumber; trycount++)
                {
                    try
                    {
                        string page;
                        try
                        {
                            await totalRequestsSemaphore.WaitAsync(config.CancellationToken);
                            if (config.CancellationToken.IsCancellationRequested)
                            {
                                break;
                            }

                            var result = await config.HttpGrabber.Grab(job.Uri, job.Referrer);

                            lastCode = result.Status;
                            if (config.RetryErrors.Contains(lastCode) || (result.Content == null))
                            {
                                await Task.Delay(config.RequestErrorRetryDelay);
                                continue;
                            }

                            page = result.Content;
                        }
                        finally
                        {
                            totalRequestsSemaphore.Release();
                        }

                        var html = new HtmlDocument();
                        html.LoadHtml(page);

                        (var nofollow, var noindex) = CheckNofollowNoindex(html);

                        var trace = new StringBuilder(job.Uri.ToString()).Append(" -");

                        if (!noindex)
                        {
                            var metadata = ExtractMetadata(html);
                            await storage.DumpUriContent(
                                job.OwnerId,
                                job.SessionId,
                                job.Uri.ToString(),
                                new MemoryStream(Encoding.UTF8.GetBytes(page)),
                                config.CancellationToken,
                                metadata);
                        }
                        else
                        {
                            trace.Append(" NOFOLLOW is in force: Skip Indexing.");
                        }

                        UriCrawled?.Invoke(this, job.Uri.ToString());

                        if (!nofollow)
                        {
                            await ParseLinks(job, html);
                        }
                        else
                        {
                            trace.Append(" NOINDEX is in force: Skip Indexing.");
                        }

                        if (!nofollow && !noindex)
                        {
                            trace.Append(" OK");
                        }

                        config.Logger.Trace(trace.ToString());

                        lastException = null;

                        break;
                    }
                    catch (TaskCanceledException ex)
                    {
                        config.Logger.Error($"{job.Uri} - Retry {trycount} - Timeout");
                        if (config.CancellationToken.IsCancellationRequested)
                        {
                            return;
                        }

                        lastException = ex;
                    }
                    catch (Exception ex)
                    {
                        config.Logger.Error($"{job.Uri} - Retry {trycount} - Failed : ", ex);
                        lastException = ex;
                        await Task.Delay(config.RequestErrorRetryDelay);
                    }
                }

                if (lastException != null)
                {
                    throw lastException;
                }

                await config.Storage.StorePageError(job.OwnerId, job.SessionId, job.Uri.ToString(), lastCode);
            }
            catch (TaskCanceledException)
            {
                config.Logger.Error($"{job.Uri} - Timeout");
            }
            catch (Exception ex)
            {
                config.Logger.Error($"{job.Uri} - Failed : ", ex);
            }
        }

        private async Task InternalRunParsersJobs()
        {
            while (!config.CancellationToken.IsCancellationRequested)
            {
                try
                {
                    var job = await config.Queue.DequeueAsync(config.CancellationToken);
                    await InternalIncite(job);
                    await job.Commit();
                }
                catch (OperationCanceledException)
                {
                    // Ignore
                }
                catch (Exception ex)
                {
                    config.Logger.Error(ex);
                }
            }
        }

        private async Task ParseLinks(IParserJob state, HtmlDocument html)
        {
            foreach (var link in linkParser.ParseLinks(html))
            {
                if (config.CancellationToken.IsCancellationRequested)
                {
                    break;
                }

                if (Uri.TryCreate(link, UriKind.RelativeOrAbsolute, out var linkuri))
                {
                    if (linkuri.IsAbsoluteUri)
                    {
                        if (linkuri.Scheme.StartsWith("http"))
                        {
                            linkuri = new Uri(linkuri.GetComponents(
                                                  UriComponents.SchemeAndServer |
                                                  UriComponents.UserInfo |
                                                  UriComponents.PathAndQuery,
                                                  UriFormat.UriEscaped)); // to remove address fragment

                            await AddUrl(state, linkuri);
                        }
                    }
                    else
                    {
                        linkuri = new Uri(state.Host, linkuri);
                        linkuri = new Uri(linkuri.GetComponents(
                                              UriComponents.SchemeAndServer |
                                              UriComponents.UserInfo |
                                              UriComponents.PathAndQuery,
                                              UriFormat.UriEscaped)); // to remove address fragment
                    }

                    await AddUrl(state, linkuri);
                }
            }
        }
    }
}