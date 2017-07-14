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
    using Logger;
    using Nito.AsyncEx;
    using RobotsTxt;

    /// <summary>
    /// Crawls, parses and indexing web pages.
    /// </summary>
    public class Crawler
    {
        private readonly HttpClient client;

        private readonly Configuration config;
        private readonly AsyncAutoResetEvent lastEvent;
        private readonly ConcurrentDictionary<Uri, Task<Robots>> robots;
        private readonly ICrawlerStorage storage;
        private readonly ConcurrentDictionary<Uri, QueuedTaskRunner> taskRunners;
        private readonly ConcurrentBag<Task> tasks;

        private readonly AsyncSemaphore totalRequestsSemaphore;
        private readonly HashSet<string> visited;
        private readonly object visitedLock = new object();
        private int countdown;
        private string sessionid;

        /// <summary>
        /// Initializes a new instance of the <see cref="Crawler" /> class.
        /// </summary>
        /// <param name="conf">Configuration for crawler.</param>
        public Crawler(Configuration conf = null)
        {
            EncodingRedirector.RegisterEncodings();

            config = new Configuration(conf) ?? new Configuration();

            client = new HttpClient
            {
                Timeout = config.RequestTimeout
            };

            client.DefaultRequestHeaders.UserAgent.ParseAdd(config.UserAgent);
            storage = config.Storage;

            tasks = new ConcurrentBag<Task>();
            robots = new ConcurrentDictionary<Uri, Task<Robots>>();
            taskRunners = new ConcurrentDictionary<Uri, QueuedTaskRunner>();
            visited = new HashSet<string>();
            lastEvent = new AsyncAutoResetEvent(false);

            config.CancellationToken.Register(() => lastEvent.Set());

            totalRequestsSemaphore = new AsyncSemaphore(config.NumberOfSimulataneousRequests);
        }

        /// <summary>
        /// Called when crawler parsed and dumped new page
        /// </summary>
        public event Action<string> UriCrawled;

        /// <summary>
        /// Starts crawling by single URI
        /// </summary>
        /// <param name="uri">URI to crawl.</param>
        /// <returns>A <see cref="Task" /> representing the asynchronous operation.</returns>
        public async Task<string> Incite(Uri uri)
        {
            sessionid = await storage.CreateSession(new[] { uri.ToString() });
            await AddUrl(null, uri, 0, 0);

            await WaitForTheEnd();
            return sessionid;
        }

        private async Task WaitForTheEnd()
        {
            await lastEvent.WaitAsync(config.CancellationToken);
            await Task.WhenAll(tasks);
            config.CancellationToken.ThrowIfCancellationRequested();
        }

        /// <summary>
        /// Starts crawling by collection of URIs
        /// </summary>
        /// <param name="uris">URIs to crawl.</param>
        /// <returns>A <see cref="Task" /> representing the asynchronous operation.</returns>
        public async Task<string> Incite(IList<Uri> uris)
        {
            sessionid = await storage.CreateSession(uris.Select(u => u.ToString()));
            foreach (var uri in uris)
            {
                await AddUrl(null, uri, 0, 0);
            }

            await WaitForTheEnd();
            return sessionid;
        }

        private async Task AddUrl(State state, Uri uri, int depth, int hostDepth)
        {
            if (depth > config.Depth || hostDepth > config.HostDepth)
            {
                return;
            }

            if (state != null)
            {
                await config.Storage.AddPageReferer(sessionid, uri.ToString(), state.Uri.ToString());
            }

            lock (visitedLock)
            {
                if (!visited.Add(uri.ToString()))
                {
                    return;
                }
            }

            var newstate = new State
            {
                Uri = uri,
                Depth = depth,
                HostDepth = hostDepth,
                Referrer = state?.Uri
            };

            var robotstxt = await GetRobotsTxt(newstate.Host);
            if (robotstxt?.IsPathAllowed(config.UserAgent, uri.PathAndQuery) == false)
            {
                return;
            }

            Interlocked.Increment(ref countdown);
            var runner = taskRunners.GetOrAdd(
                newstate.Host,
                host => new QueuedTaskRunner(config.HostRequestsDelay, config.CancellationToken));
            var task = new Task(async () => await InnerIncite(newstate));
            tasks.Add(task);
            runner.Enqueue(task);
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
                        return new Robots(robotstxt);
                    }
                    catch (HttpRequestException)
                    {
                        return null;
                    }
                });
        }

        private async Task InnerIncite(State state)
        {
            try
            {
                Exception lastException = null;
                var lastCode = HttpStatusCode.OK;
                var nofollow = false;
                var noindex = false;

                for (var trycount = 0; trycount < config.RetriesNumber; trycount++)
                {
                    try
                    {
                        byte[] page;
                        try
                        {
                            await totalRequestsSemaphore.WaitAsync(config.CancellationToken);
                            if (config.CancellationToken.IsCancellationRequested)
                            {
                                break;
                            }

                            var request = new HttpRequestMessage(HttpMethod.Get, state.Uri);
                            if (state.Referrer != null)
                            {
                                request.Headers.Referrer = state.Referrer;
                            }

                            var result = await client.SendAsync(request, config.CancellationToken);
                            if (!result.IsSuccessStatusCode)
                            {
                                lastCode = result.StatusCode;
                                config.Logger.Error(
                                    $"{state.Uri} - HttpError: {result.StatusCode}{(int)result.StatusCode}");
                                await Task.Delay(config.RequestErrorRetryDelay);

                                // TODO process error;
                                continue;
                            }

                            lastCode = HttpStatusCode.OK;
                            page = await result.Content.ReadAsByteArrayAsync();
                        }
                        finally
                        {
                            totalRequestsSemaphore.Release();
                        }

                        var html = new HtmlDocument();
                        html.Load(new MemoryStream(page));

                        foreach (var meta in html.DocumentNode.SelectNodes("//meta[name='robots']")?
                                                 .Select(m => m.Attributes["content"].Value) ?? new string[0])
                        {
                            if (meta.IndexOf("NOFOLLOW", StringComparison.InvariantCultureIgnoreCase) >= 0)
                            {
                                nofollow = true;
                            }

                            if (meta.IndexOf("NOINDEX", StringComparison.InvariantCultureIgnoreCase) >= 0)
                            {
                                noindex = true;
                            }

                            if (nofollow && noindex)
                            {
                                break;
                            }
                        }

                        var trace = new StringBuilder(state.Uri.ToString()).Append(" -");

                        if (!noindex)
                        {
                            await storage.DumpPage(state.Uri.ToString(), new MemoryStream(page));
                        }
                        else
                        {
                            trace.Append(" NOFOLLOW is in force: Skip Indexing.");
                        }

                        if (!nofollow)
                        {
                            await ParseLinks(state, html);
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

                        UriCrawled?.Invoke(state.Uri.ToString());
                        break;
                    }
                    catch (TaskCanceledException ex)
                    {
                        if (config.CancellationToken.IsCancellationRequested)
                        {
                            return;
                        }

                        lastException = ex;
                    }
                    catch (Exception ex)
                    {
                        lastException = ex;
                        await Task.Delay(config.RequestErrorRetryDelay);
                    }
                }

                if (lastException != null)
                {
                    throw lastException;
                }

                await config.Storage.StorePageError(sessionid, state.Uri.ToString(), lastCode);
            }
            catch (TaskCanceledException)
            {
                config.Logger.Error($"{state.Uri} - Timeout");
            }
            catch (Exception ex)
            {
                config.Logger.Error($"{state.Uri} - Failed : ", ex);
            }
            finally
            {
                if (Interlocked.Decrement(ref countdown) == 0)
                {
                    lastEvent.Set();
                }
            }
        }

        private async Task ParseLinks(State state, HtmlDocument html)
        {
            var links = html.DocumentNode.SelectNodes("//a")
                            ?.Select(l => l.Attributes["href"]).Where(l => l != null)
                            .Select(l => l.Value).Where(l => !string.IsNullOrWhiteSpace(l))
                            .ToList() ?? new List<string>();

            foreach (var link in links)
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
                            var newhostUri =
                                new Uri(linkuri.GetComponents(
                                            UriComponents.SchemeAndServer,
                                            UriFormat.UriEscaped));
                            linkuri = new Uri(linkuri.GetComponents(
                                                  UriComponents.SchemeAndServer
                                                  | UriComponents.UserInfo
                                                  | UriComponents.PathAndQuery,
                                                  UriFormat.UriEscaped)); // remove fragment

                            await AddUrl(
                                state,
                                linkuri,
                                state.Depth + 1,
                                state.HostDepth + (state.Host == newhostUri ? 0 : 1));
                        }
                    }
                    else
                    {
                        linkuri = new Uri(state.Host, linkuri);
                        linkuri = new Uri(linkuri.GetComponents(
                                              UriComponents.SchemeAndServer
                                              | UriComponents.UserInfo
                                              | UriComponents.PathAndQuery,
                                              UriFormat.UriEscaped)); // remove fragment

                        await AddUrl(state, linkuri, state.Depth + 1, state.HostDepth);
                    }
                }
            }
        }

        private class State
        {
            private Uri uri;

            public Uri Uri
            {
                get => uri;
                set
                {
                    uri = value;
                    Host = new Uri(uri.GetComponents(UriComponents.SchemeAndServer, UriFormat.UriEscaped));
                    UriHostAndPort = uri.GetComponents(UriComponents.HostAndPort, UriFormat.UriEscaped);
                }
            }

            public string UriHostAndPort { get; private set; }

            public Uri Host { get; private set; }

            public int Depth { get; set; }

            public int HostDepth { get; set; }

            public Uri Referrer { get; set; }
        }
    }
}