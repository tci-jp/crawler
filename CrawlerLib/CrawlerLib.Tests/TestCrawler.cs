// <copyright file="TestCrawler.cs" company="DECTech.Tokyo">
// Copyright (c) DECTech.Tokyo. All rights reserved.
// </copyright>

namespace CrawlerLib.Tests
{
    using System;
    using System.Collections.Async;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using Azure;
    using FluentAssertions;
    using global::Azure.Storage;
    using Grabbers;
    using Microsoft.Extensions.Configuration;
    using Xunit;
    using Xunit.Abstractions;

    public class TestCrawler
    {
        private readonly Configuration crawlerConfig = new Configuration();
        private readonly ITestOutputHelper output;

        private Crawler crawler;

        public TestCrawler(ITestOutputHelper output)
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

        private Crawler Crawler => crawler ?? NewCrawler();

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

            var session = await Crawler.InciteStart("unittest", new[] { new Uri(url) });

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

            var session = await Crawler.InciteStart("unittest", new[] { new Uri(url) });

            await session.CrawlerTask;

            var urls = await crawlerConfig.Storage.GetSessionUris(session.SessionId).ToListAsync();
            urls.Select(u => u.Uri.ToString()).Should().BeEquivalentTo(url);

            var stream = new MemoryStream();
            await crawlerConfig.Storage.GetUriContet("unittest", url, stream);
            stream.Position = 0;
            var str = new StreamReader(stream).ReadToEnd();
            str.Should().Contain("<body").And.Contain("</body>");
        }

        private Crawler NewCrawler()
        {
            crawler = new Crawler(crawlerConfig);
            return crawler;
        }
    }
}