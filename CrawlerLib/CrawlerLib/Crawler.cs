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
    using Queue;

    /// <inheritdoc />
    /// <summary>
    /// Crawls, parses and indexes web pages.
    /// </summary>
    public sealed class Crawler : ICrawler
    {
        private readonly Configuration config;

        private readonly ILinkParser linkParser = new LinkParser();

        private readonly ConcurrentDictionary<Uri, Task<IRobots>> robots;
        private readonly ICrawlerStorage storage;

        private readonly SemaphoreSlim totalRequestsSemaphore;

        /// <summary>
        /// Initializes a new instance of the <see cref="Crawler" /> class.
        /// </summary>
        /// <param name="conf">Configuration for crawler.</param>
        public Crawler(Configuration conf = null)
        {
            // EncodingRedirector.RegisterEncodings();
            config = new Configuration(conf);
            storage = config.Storage;

            robots = new ConcurrentDictionary<Uri, Task<IRobots>>();

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
            // client?.Dispose();
        }

        /// <summary>
        /// Starts crawling by single URI
        /// </summary>
        /// <param name="ownerId">Session onwer Id.</param>
        /// <param name="uri">URI to crawl.</param>
        /// <returns>Session Id. A <see cref="Task" /> representing the asynchronous operation.</returns>
        [UsedImplicitly]
        [Obsolete("Use InciteStart instead. May not work properly.")]
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
        [Obsolete("Use InciteStart instead. May not work properly.")]
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

        /// <inheritdoc />
        public async Task InciteJob(ICommitableParserJob job, CancellationToken cancellation)
        {
            try
            {
                var sessionInfo = await storage.GetSingleSession(job.OwnerId, job.SessionId);
                if ((sessionInfo.State == SessionState.Cancelled) || ((sessionInfo.CancellationTime != null) && (DateTime.UtcNow > sessionInfo.CancellationTime)))
                {
                    await storage.UpdateSessionState(job.OwnerId, job.SessionId, SessionState.Cancelled);
                    await job.Commit(cancellation, (int)SessionState.Cancelled);
                    return;
                }

                await storage.UpdateSessionUri(job.SessionId, job.Uri.ToString(), (int)SessionState.InProcess);
                var lastCode = HttpStatusCode.OK;

                string page = null;
                try
                {
                    await totalRequestsSemaphore.WaitAsync(cancellation);
                    cancellation.ThrowIfCancellationRequested();

                    for (var retry = 0; retry < config.RetriesNumber; retry++)
                    {
                        var result = await config.HttpGrabber.Grab(job.Uri, job.Referrer);

                        lastCode = result.Status;
                        if (config.RetryErrors.Contains(lastCode) || (result.Content == null))
                        {
                            await Task.Delay(config.RequestErrorRetryDelay, cancellation);
                            continue;
                        }

                        page = result.Content;
                        break;
                    }
                }
                finally
                {
                    totalRequestsSemaphore.Release();
                }

                var html = new HtmlDocument();
                html.LoadHtml(page);

                CheckNofollowNoindex(html, out var nofollow, out var noindex);

                var trace = new StringBuilder(job.Uri.ToString()).Append(" -");

                if (!noindex)
                {
                    var metadata = ExtractMetadata(html, job);
                    await storage.DumpUriContent(
                        job.OwnerId,
                        job.SessionId,
                        job.Uri.ToString(),
                        new MemoryStream(Encoding.UTF8.GetBytes(page)),
                        cancellation,
                        metadata);
                }
                else
                {
                    trace.Append(" NOFOLLOW is in force: Skip Indexing.");
                }

                UriCrawled?.Invoke(this, job.Uri.ToString());

                if (!nofollow)
                {
                    await ParseLinks(job, html, cancellation);
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

                await job.Commit(cancellation, (int)lastCode);
            }
            catch (Exception ex)
            {
                await job.Commit(cancellation, -1, ex.ToString());
                config.Logger.Error($"{job.Uri} - Failed : ", ex);
                throw;
            }
        }

        /// <inheritdoc />
        [UsedImplicitly]
        public async Task<string> InciteStart(
            string ownerId,
            IEnumerable<UriParameter> uris,
            DateTime? cancellationTime = null,
            QueueParserParameters parserParameters = null)
        {
            var urisList = new List<UriParameter>(uris);
            if (urisList.Count == 0)
            {
                return null;
            }

            var sessionid = await storage.CreateSession(ownerId, urisList.Select(u => u.ToString()), cancellationTime);
            try
            {
                foreach (var uri in urisList)
                {
                    try
                    {
                        var newjob = new ParserJob
                        {
                            OwnerId = ownerId,
                            SessionId = sessionid,
                            Uri = new Uri(uri.Uri),
                            Depth = uri.Depth ?? config.Depth,
                            HostDepth = uri.HostDepth ?? config.HostDepth,
                            ParserParameters = parserParameters
                        };
                        await EnqueueAsync(newjob);
                    }
                    catch (Exception ex)
                    {
                        await storage.UpdateSessionUri(sessionid, uri.Uri, (int)SessionState.Error, ex.ToString());
                    }
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
                var task = Task.Factory.StartNew(
                    async () => { await InternalRunParsersJobs(); },
                    config.CancellationToken,
                    TaskCreationOptions.LongRunning,
                    TaskScheduler.Default);
            }
        }

        private static void CheckNofollowNoindex(HtmlDocument html, out bool nofollow, out bool noindex)
        {
            nofollow = false;
            noindex = false;
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
        }

        private async Task AddUrl(IParserJob parent, Uri newUri, CancellationToken cancellation)
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

            var robotstxt = await GetRobotsTxt(newjob.Host, cancellation);
            if (robotstxt?.IsPathAllowed(newjob.Uri.PathAndQuery) == false)
            {
                return;
            }

            await config.Queue.EnqueueAsync(newjob, config.CancellationToken);
        }

        private async Task EnqueueAsync(IParserJob newjob)
        {
            await config.Queue.EnqueueAsync(newjob, config.CancellationToken);
        }

        private IEnumerable<KeyValuePair<string, string>> ExtractMetadata(HtmlDocument html, IParserJob job)
        {
            return job.ParserParameters?.GetExtractors().SelectMany(ex => ex.ExtractMetadata(html))
                ?? config.MetadataExtractors.SelectMany(ex => ex.ExtractMetadata(html));
        }

        private Task<IRobots> GetRobotsTxt(Uri host, CancellationToken cancellation)
        {
            return robots.GetOrAdd(
                host,
                async roburi =>
                {
                    try
                    {
                        return await config.RobotstxtFactory.RetrieveAsync(new Uri(roburi + "/robots.txt"), cancellation);
                    }
                    catch (HttpRequestException)
                    {
                        return null;
                    }
                });
        }

        private async Task InternalRunParsersJobs()
        {
            while (!config.CancellationToken.IsCancellationRequested)
            {
                try
                {
                    using (var timeout = new CancellationTokenSource(config.CrawlerJobTimeout))
                    using (var cancellation =
                        CancellationTokenSource.CreateLinkedTokenSource(config.CancellationToken, timeout.Token))
                    {
                        var job = await config.Queue.DequeueAsync(cancellation.Token);
                        await InciteJob(job, cancellation.Token);
                    }
                }
                catch (OperationCanceledException) when (config.CancellationToken.IsCancellationRequested)
                {
                    // Ignore
                }
                catch (Exception ex)
                {
                    config.Logger.Error(ex);
                }
            }
        }

        private async Task ParseLinks(IParserJob state, HtmlDocument html, CancellationToken cancellation)
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

                            await AddUrl(state, linkuri, cancellation);
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

                    await AddUrl(state, linkuri, cancellation);
                }
            }
        }
    }
}