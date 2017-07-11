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
    using System.Threading;
    using System.Threading.Tasks;
    using HtmlAgilityPack;
    using Logger;
    using Nito.AsyncEx;

    public class Crawler
    {
        private readonly HttpClient client;
        private readonly ICrawlerStorage storage;
        private int countdown;
        private AsyncManualResetEvent lastEvent;
        private ConcurrentDictionary<string, Task<RobotsTxt>> robots;
        private ConcurrentBag<Task> tasks;
        private HashSet<string> visited;

        public Crawler(Configuration con = null)
        {
            Config = con ?? new Configuration();

            client = new HttpClient();
            client.Timeout = Config.RequestTimeout;
            client.DefaultRequestHeaders.UserAgent.ParseAdd(Config.UserAgent);
            storage = Config.Storage;
        }

        public Configuration Config { get; }

        public async Task Incite(Uri uri)
        {
            tasks = new ConcurrentBag<Task>();
            robots = new ConcurrentDictionary<string, Task<RobotsTxt>>();
            visited = new HashSet<string>();
            lastEvent = new AsyncManualResetEvent(false);
            AddUrl(uri, 0, 0);
            await lastEvent.WaitAsync();
            await Task.WhenAll(tasks);
        }

        private void AddUrl(Uri uri, int depth, int hostDepth)
        {
            if (depth > Config.Depth || hostDepth > Config.HostDepth)
            {
                return;
            }

            lock (visited)
            {
                if (!visited.Add(uri.ToString()))
                {
                    return;
                }
            }

            Interlocked.Increment(ref countdown);
            tasks.Add(Task.Run(() => InnerIncite(uri, depth, hostDepth)));
        }

        private Task<RobotsTxt> GetRobotsTxt(Uri host)
        {
            return robots.GetOrAdd(host.ToString(), async (roburi) =>
            {
                try
                {
                    var robotstxt = await client.GetStringAsync(roburi + "/robots.txt");
                    return new RobotsTxt(robotstxt);
                }
                catch (HttpRequestException)
                {
                    return RobotsTxt.DefaultInstance;
                }
            });
        }

        private async Task InnerIncite(Uri uri, int depth, int hostDepth)
        {
            try
            {
                var hostUri = new Uri(uri.GetComponents(UriComponents.SchemeAndServer, UriFormat.UriEscaped));

                var robotstxt = await GetRobotsTxt(hostUri);
                Exception lastException = null;
                for (var trycount = 0; trycount < Config.RetriesNumber; trycount++)
                {
                    try
                    {
                        var result = await client.GetAsync(uri);
                        if (!result.IsSuccessStatusCode)
                        {
                            Config.Logger.Error($"{uri} - HttpError: {result.StatusCode}{(int)result.StatusCode}");
                            await Task.Delay(Config.RequestErrorRetryDelay);

                            // TODO process error;
                            continue;
                        }

                        var page = await result.Content.ReadAsByteArrayAsync();
                        await storage.DumpPage(uri.ToString(), new MemoryStream(page));

                        var html = new HtmlDocument();
                        html.Load(new MemoryStream(page));
                        var links = html.DocumentNode.SelectNodes("//a")
                                        ?.Select(l => l.Attributes["href"]).Where(l => l != null)
                                        .Select(l => l.Value).Where(l => !string.IsNullOrWhiteSpace(l))
                                        .ToList() ?? new List<string>();

                        foreach (var link in links)
                        {
                            if (Uri.TryCreate(link, UriKind.RelativeOrAbsolute, out var linkuri))
                            {
                                if (!linkuri.IsAbsoluteUri)
                                {
                                    linkuri = new Uri(hostUri, linkuri);
                                    linkuri = new Uri(linkuri.GetComponents(
                                                          UriComponents.SchemeAndServer
                                                          | UriComponents.UserInfo
                                                          | UriComponents.PathAndQuery,
                                                          UriFormat.UriEscaped)); // remove fragment

                                    AddUrl(linkuri, depth + 1, hostDepth);
                                }
                                else
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

                                    AddUrl(linkuri, depth + 1, hostDepth + (hostUri == newhostUri ? 0 : 1));
                                }
                            }
                        }

                        Config.Logger.Trace($"{uri} - OK");
                        break;
                    }
                    catch (TaskCanceledException ex)
                    {
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
                Config.Logger.Error($"{uri} - Timeout");
            }
            catch (Exception ex)
            {
                Config.Logger.Error($"{uri} - Failed : ", ex);
            }
            finally
            {
                if (Interlocked.Decrement(ref countdown) == 0)
                {
                    lastEvent.Set();
                }
            }
        }
    }
}