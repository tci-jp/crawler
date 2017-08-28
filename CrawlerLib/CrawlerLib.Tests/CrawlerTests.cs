// <copyright file="TestCrawler.cs" company="DECTech.Tokyo">
// Copyright (c) DECTech.Tokyo. All rights reserved.
// </copyright>

namespace CrawlerLib.Tests
{
    using System;
    using System.Collections.Async;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using Azure;
    using CrawlerLib.Metadata;
    using Data;
    using FluentAssertions;
    using global::Azure.Storage;
    using Grabbers;
    using Microsoft.Extensions.Configuration;
    using Xunit;
    using Xunit.Abstractions;

    public class CrawlerTests
    {
        private const string Owner = "unittest";
        private readonly Configuration crawlerConfig = new Configuration();
        private readonly ITestOutputHelper output;

        private Crawler crawler;

        public CrawlerTests(ITestOutputHelper output)
        {
            this.output = output;

            var builder = new ConfigurationBuilder()
                .AddJsonFile("config.json", true, true)
                .AddEnvironmentVariables();
            var configuration = builder.Build();

            crawlerConfig.HttpGrabber = new WebDriverHttpGrabber(crawlerConfig);
            var storage = new DataStorage(configuration["CrawlerStorageConnectionString"]);
            var searcher = new AzureIndexedSearch(
                storage,
                configuration["SearchServiceName"],
                configuration["SearchServiceAdminApiKey"],
                configuration["TextSearchIndexName"],
                configuration["MetaSearchIndexName"]);

            crawlerConfig.Storage = new AzureCrawlerStorage(storage, searcher);
        }

        private Crawler Crawler => crawler ?? (crawler = new Crawler(crawlerConfig));

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

            var session = await Crawler.InciteStart(Owner, new[] { new Uri(url) });

            await session.CrawlerTask;

            var urls = await crawlerConfig.Storage.GetSessionUris(session.SessionId).ToListAsync();
            urls.Select(u => u.Uri.ToString()).Should().BeEquivalentTo(result);
        }

        [Theory]
        [InlineData("http://www.dectech.tokyo/")]
        public async Task TestContent(string url)
        {
            crawlerConfig.HostDepth = 0;
            crawlerConfig.Depth = 0;

            var session = await Crawler.InciteStart(Owner, new[] { new Uri(url) });

            var sesspage = await crawlerConfig.Storage.GetSessions(Owner, new[] { session.SessionId });
            sesspage.Items.Count().Should().Be(1);
            var sessinfo = sesspage.Items.Single();
            sessinfo.State.Should().Be(SessionState.InProcess);

            await session.CrawlerTask;

            sesspage = await crawlerConfig.Storage.GetSessions(Owner, new[] { session.SessionId });
            sesspage.Items.Count().Should().Be(1);
            sessinfo = sesspage.Items.Single();
            sessinfo.State.Should().Be(SessionState.Done);

            var urls = await crawlerConfig.Storage.GetSessionUris(session.SessionId).ToListAsync();
            urls.Select(u => u.Uri.ToString()).Should().BeEquivalentTo(url);

            var stream = new MemoryStream();
            await crawlerConfig.Storage.GetUriContet(Owner, url, stream);
            stream.Position = 0;
            var str = new StreamReader(stream).ReadToEnd();
            str.Should().Contain("<body").And.Contain("</body>");
        }

        [Theory]
        [InlineData("fn:match(string(//div/@id),'(\\d+)')", "http://www.dectech.tokyo/", new[]
                                                      {
                                                         "02"
                                                      })]
        [InlineData("fn:match(string(//input/@id), '(\\D+)')", "http://www.dectech.tokyo/", new[]
                                                      {
                                                         "q"
                                                      })]
        [InlineData("fn:match(string(//h1/@class), '(\\D+\\d+)')", "http://www.dectech.tokyo/", new[]
                                                      {
                                                         "display-4"
                                                      })]
        public async Task TestCustomParsing(string xpath, string url, string[] result)
        {
            crawlerConfig.HostDepth = 0;
            crawlerConfig.Depth = 0;
            var extractor = new XPathMetadataExtractor(new[] { new KeyValuePair<string, string>(xpath, "field") });
            crawlerConfig.MetadataExtractors = new XPathMetadataExtractor[] { extractor };

            var session = await Crawler.InciteStart(Owner, new[] { new Uri(url) });

            var sesspage = await crawlerConfig.Storage.GetSessions(Owner, new[] { session.SessionId });
            sesspage.Items.Count().Should().Be(1);
            var sessinfo = sesspage.Items.Single();
            sessinfo.State.Should().Be(SessionState.InProcess);

            await session.CrawlerTask;

            sesspage = await crawlerConfig.Storage.GetSessions(Owner, new[] { session.SessionId });
            sesspage.Items.Count().Should().Be(1);
            sessinfo = sesspage.Items.Single();
            sessinfo.State.Should().Be(SessionState.Done);

            var urls = await crawlerConfig.Storage.GetSessionUris(session.SessionId).ToListAsync();
            urls.Select(u => u.Uri.ToString()).Should().BeEquivalentTo(url);
            var resultMetadata = await crawlerConfig.Storage.GetUriMetadata(Owner, url).ToListAsync();
            resultMetadata.Select(t => t.Value.ToString()).Should().BeEquivalentTo(result);
            resultMetadata.Select(t => t.Key).Should().BeEquivalentTo(new[] { "field" });
        }
    }

}