// <copyright file="TestCrawler.cs" company="DECTech.Tokyo">
// Copyright (c) DECTech.Tokyo. All rights reserved.
// </copyright>

namespace CrawlerLib.Tests
{
    using System;
    using System.Collections.Async;
    using System.Threading.Tasks;
    using Grabbers;
    using Xunit;
    using Xunit.Abstractions;

    public class TestCrawler
    {
        private readonly Configuration config = new Configuration();
        private readonly ITestOutputHelper output;

        private Crawler crawler;

        public TestCrawler(ITestOutputHelper output)
        {
            this.output = output;
        }

        private Crawler Crawler => crawler ?? NewCrawler();

        [Theory]
        [InlineData(2, 0, "http://www.dectech.tokyo")]
        public async Task TestInciteStability(int depth, int hostDepth, string url)
        {
            config.HostDepth = hostDepth;
            config.Depth = depth;
            config.HttpGrabber = new WebDriverHttpGrabber(config);

            var id = await Crawler.Incite("test", new Uri(url));

            var urls = await config.Storage.GetSessionUris(id).ToListAsync();
            foreach (var line in urls)
            {
                output.WriteLine(line.Uri);
            }

            output.WriteLine(urls.Count.ToString());
        }

        private Crawler NewCrawler()
        {
            crawler = new Crawler(config);
            return crawler;
        }
    }
}