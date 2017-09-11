// <copyright file="CrawlerTests.cs" company="DECTech.Tokyo">
// Copyright (c) DECTech.Tokyo. All rights reserved.
// </copyright>

namespace CrawlerLib.Tests
{
    using System;
    using System.Collections.Async;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Azure;
    using Data;
    using FluentAssertions;
    using global::Azure.Storage;
    using Grabbers;
    using Metadata;
    using Microsoft.Extensions.Configuration;
    using Queue;
    using Xunit;
    using Xunit.Abstractions;

    public class CrawlerTests : IDisposable
    {
        private const string Owner = "unittest";
        private readonly CancellationTokenSource cancel = new CancellationTokenSource();
        private readonly Configuration crawlerConfig = new Configuration();
        private readonly ITestOutputHelper output;
        private readonly MemoryParserJobsQueue queue;

        private Crawler crawler;

        public CrawlerTests(ITestOutputHelper output)
        {
            this.output = output;

            var builder = new ConfigurationBuilder()
                .AddJsonFile("config.json", true, true)
                .AddEnvironmentVariables();
            var configuration = builder.Build();

            crawlerConfig.HttpGrabber = new FileGrabber(crawlerConfig);
            var storage = new DataStorage(configuration["CrawlerStorageConnectionString"]);
            var searcher = new AzureIndexedSearch(
                storage,
                configuration["SearchServiceName"],
                configuration["SearchServiceAdminApiKey"],
                configuration["TextSearchIndexName"],
                configuration["MetaSearchIndexName"]);

            var crawlerStorage = new AzureCrawlerStorage(storage, searcher);
            crawlerConfig.Storage = crawlerStorage;
            queue = new MemoryParserJobsQueue(crawlerStorage);
            crawlerConfig.Queue = queue;
            crawlerConfig.CancellationToken = cancel.Token;
        }

        private Crawler Crawler => crawler ?? GetCrawler();

        public void Dispose()
        {
            cancel.Cancel();
        }

        [Theory]
        [InlineData("http://www.dectech.tokyo/")]
        public async Task TestContent(string url)
        {
            crawlerConfig.HostDepth = 0;
            crawlerConfig.Depth = 0;

            var session = await Crawler.InciteStart(Owner, new[] { new UriParameter(url) });

            var sesspage = await crawlerConfig.Storage.GetSessions(Owner, new[] { session });
            sesspage.Items.Count().Should().Be(1);

            await queue.WaitForSession(session, crawlerConfig.CancellationToken);

            sesspage = await crawlerConfig.Storage.GetSessions(Owner, new[] { session });
            sesspage.Items.Count().Should().Be(1);
            var sessinfo = sesspage.Items.Single();
            sessinfo.State.Should().Be(SessionState.InProcess);

            var urls = await crawlerConfig.Storage.GetSessionUris(session).ToListAsync();
            urls.Select(u => u.Uri.ToString()).Should().BeEquivalentTo(url);
            urls.All(u => u.State == 200).Should().Be(true);

            using (var stream = await crawlerConfig.Storage.GetUriContent(Owner, url))
            {
                var str = new StreamReader(stream).ReadToEnd();
                str.Should().Contain("<body").And.Contain("</body>");
            }
        }

        [Theory]
        [InlineData("fn:match(//div/@class, '\\d+')", "http://www.dectech.tokyo/", new[]
                                                                                         {
                                                                                            "0",
                                                                                            "2",
                                                                                            "3"
                                                                                         })]
        [InlineData("fn:match(//a/@class, '\\D+')", "http://www.dectech.tokyo/", new[]
                                                                                            {
                                                                                                "btn btn-outline-secondary",
                                                                                                "card-footer",
                                                                                                "footer-link",
                                                                                                "navbar-brand"
                                                                                            })]
        public async Task TestCustomParsing(string xpath, string url, string[] result)
        {
            crawlerConfig.HostDepth = 0;
            crawlerConfig.Depth = 0;
            var parameters = new ParserParameters { XPathCustomFields = new[] { new XPathCustomFields { XPath = xpath, Name = "field" } } };

            var session = await Crawler.InciteStart(Owner, new[] { new UriParameter(url) }, parameters);

            var sesspage = await crawlerConfig.Storage.GetSessions(Owner, new[] { session });
            sesspage.Items.Count().Should().Be(1);
            var sessinfo = sesspage.Items.Single();
            sessinfo.State.Should().Be(SessionState.InProcess);

            await queue.WaitForSession(session, crawlerConfig.CancellationToken);

            sesspage = await crawlerConfig.Storage.GetSessions(Owner, new[] { session });
            sesspage.Items.Count().Should().Be(1);
            sessinfo = sesspage.Items.Single();
            sessinfo.State.Should().Be(SessionState.InProcess);

            var urls = await crawlerConfig.Storage.GetSessionUris(session).ToListAsync();
            urls.Select(u => u.Uri.ToString()).Should().BeEquivalentTo(url);
            urls.All(u => u.State == 200).Should().Be(true);

            var resultMetadata = await crawlerConfig.Storage.GetUriMetadata(Owner, url).ToListAsync();
            resultMetadata.Select(t => t.Value.ToString()).Should().BeEquivalentTo(result);
            resultMetadata.Select(t => t.Key).Should().Contain("field");
        }

        [Theory]
        [InlineData(0, 0, "http://www.dectech.tokyo", new[]
                                                      {
                                                          "http://www.dectech.tokyo/"
                                                      })]
        [InlineData(0, 1, "http://www.dectech.tokyo", new[]
                                                      {
                                                          "http://www.dectech.tokyo/Home/C/ui-ux-engineer/",
                                                          "http://www.dectech.tokyo/Home/C/aboutus",
                                                          "http://www.dectech.tokyo/Home/C/c-sharp-engineer/",
                                                          "http://www.dectech.tokyo/Home/C/data-communication-scientist/",
                                                          "http://www.dectech.tokyo/Home/C/fastlane",
                                                          "http://www.dectech.tokyo/Home/C/fusions",
                                                          "http://www.dectech.tokyo/Home/C/privacypolicy",
                                                          "http://www.dectech.tokyo/blog/Index",
                                                          "http://www.dectech.tokyo/"
                                                      })]
        public async Task TestInciteFormalResults(int hostDepth, int depth, string url, string[] result)
        {
            crawlerConfig.HostDepth = hostDepth;
            crawlerConfig.Depth = depth;

            var session = await Crawler.InciteStart(Owner, new[] { new UriParameter(url, depth, hostDepth) });

            await queue.WaitForSession(session, crawlerConfig.CancellationToken);

            var urls = await crawlerConfig.Storage.GetSessionUris(session).ToListAsync();
            urls.Select(u => u.Uri.ToString()).Should().BeEquivalentTo(result);
        }

        private Crawler GetCrawler()
        {
            crawler = new Crawler(crawlerConfig);
            crawler.RunParserWorkers(1);
            return crawler;
        }
    }
}