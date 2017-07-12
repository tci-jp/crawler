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
    using System.Net.Http;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using HtmlAgilityPack;
    using Logger;
    using Nito.AsyncEx;

    public class Crawler
    {
        private readonly HttpClient client;
        private readonly AsyncManualResetEvent lastEvent;
        private readonly ConcurrentDictionary<Uri, Task<RobotsTxt>> robots;
        private readonly ICrawlerStorage storage;
        private readonly ConcurrentDictionary<Uri, QueuedTaskRunner> taskRunners;
        private readonly ConcurrentBag<Task> tasks;

        private readonly AsyncSemaphore totalRequestsSemaphore;
        private readonly HashSet<string> visited;
        private readonly object visitedLock = new object();
        private int countdown;

        public Crawler(Configuration con = null)
        {
            EncodingRedirector.RegisterEncodings();

            Config = con ?? new Configuration();

            client = new HttpClient
            {
                Timeout = Config.RequestTimeout
            };

            client.DefaultRequestHeaders.UserAgent.ParseAdd(Config.UserAgent);
            storage = Config.Storage;

            tasks = new ConcurrentBag<Task>();
            robots = new ConcurrentDictionary<Uri, Task<RobotsTxt>>();
            taskRunners = new ConcurrentDictionary<Uri, QueuedTaskRunner>();
            visited = new HashSet<string>();
            lastEvent = new AsyncManualResetEvent(false);

            Config.CancellationToken.Register(() => lastEvent.Set());

            totalRequestsSemaphore = new AsyncSemaphore(Config.NumberOfSimulataneousRequests);
        }

        public Configuration Config { get; }

        public async Task Incite(Uri uri)
        {
            await AddUrl(null, uri, 0, 0);

            await lastEvent.WaitAsync();
            await Task.WhenAll(tasks);
            Config.CancellationToken.ThrowIfCancellationRequested();
        }

        public async Task Incite(IEnumerable<Uri> uris)
        {
            foreach (var uri in uris)
            {
                await AddUrl(null, uri, 0, 0);
            }

            await lastEvent.WaitAsync();
            await Task.WhenAll(tasks);
            Config.CancellationToken.ThrowIfCancellationRequested();
        }


        private async Task AddUrl(State state, Uri uri, int depth, int hostDepth)
        {
            if (depth > Config.Depth || hostDepth > Config.HostDepth)
            {
                return;
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

            Interlocked.Increment(ref countdown);
            var runner = taskRunners.GetOrAdd(
                newstate.Host,
                host => new QueuedTaskRunner(Config.HostRequestsDelay, Config.CancellationToken));
            var task = new Task(async () => await InnerIncite(newstate));
            tasks.Add(task);
            runner.Enqueue(task);
        }

        private Task<RobotsTxt> GetRobotsTxt(Uri host)
        {
            return robots.GetOrAdd(
                host,
                async roburi =>
                {
                    try
                    {
                        var robotstxt =
                            await client.GetStringAsync(roburi + "/robots.txt");
                        return new RobotsTxt(robotstxt);
                    }
                    catch (HttpRequestException)
                    {
                        return RobotsTxt.DefaultInstance;
                    }
                });
        }

        private async Task InnerIncite(State state)
        {
            try
            {
                Exception lastException = null;
                var nofollow = false;
                var noindex = false;

                for (var trycount = 0; trycount < Config.RetriesNumber; trycount++)
                {
                    try
                    {
                        byte[] page;
                        try
                        {
                            await totalRequestsSemaphore.WaitAsync(Config.CancellationToken);
                            if (Config.CancellationToken.IsCancellationRequested)
                            {
                                break;
                            }

                            var request = new HttpRequestMessage(HttpMethod.Get, state.Uri);
                            if (state.Referrer != null)
                            {
                                request.Headers.Referrer = state.Referrer;
                            }

                            var result = await client.SendAsync(request, Config.CancellationToken);
                            if (!result.IsSuccessStatusCode)
                            {
                                Config.Logger.Error(
                                    $"{state.Uri} - HttpError: {result.StatusCode}{(int)result.StatusCode}");
                                await Task.Delay(Config.RequestErrorRetryDelay);

                                // TODO process error;
                                continue;
                            }

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

                        Config.Logger.Trace(trace.ToString());

                        lastException = null;
                        break;
                    }
                    catch (TaskCanceledException ex)
                    {
                        if (Config.CancellationToken.IsCancellationRequested)
                        {
                            return;
                        }
                        
                        lastException = ex;
                    }
                    catch (Exception ex)
                    {
                        lastException = ex;
                        await Task.Delay(Config.RequestErrorRetryDelay);
                    }
                }

                if (lastException != null)
                {
                    throw lastException;
                }
            }
            catch (TaskCanceledException)
            {
                Config.Logger.Error($"{state.Uri} - Timeout");
            }
            catch (Exception ex)
            {
                Config.Logger.Error($"{state.Uri} - Failed : ", ex);
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
                if (Config.CancellationToken.IsCancellationRequested)
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
                }
            }

            public Uri Host { get; private set; }

            public int Depth { get; set; }

            public int HostDepth { get; set; }

            public Uri Referrer { get; set; }
        }
    }
}