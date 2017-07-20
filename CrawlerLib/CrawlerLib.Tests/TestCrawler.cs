// <copyright file="TestCrawler.cs" company="DECTech.Tokyo">
// Copyright (c) DECTech.Tokyo. All rights reserved.
// </copyright>

namespace CrawlerLib.Tests
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Grabbers;
    using Xunit;
    using Xunit.Abstractions;

    public class TestCrawler
    {
        private readonly Configuration config = new Configuration();
        private readonly ITestOutputHelper output;

        private Crawler crawler;

        private string sessionId;

        public TestCrawler(ITestOutputHelper output)
        {
            this.output = output;
        }

        private Crawler Crawler => crawler ?? NewCrawler();

        [Theory]
        [InlineData(1, 0, "http://www.dectech.tokyo")]
        public async Task TestInciteStability(int depth, int hostDepth, string url)
        {
            config.HostDepth = hostDepth;
            config.Depth = depth;

            var id = await Crawler.Incite(new Uri(url));

            var urls = (await config.Storage.GetSessionUris(id)).ToList();
            foreach (var line in urls)
            {
                output.WriteLine(line);
            }

            output.WriteLine(urls.Count.ToString());
        }

        private Crawler NewCrawler()
        {
            crawler = new Crawler(new SimpleHttpGrabber(config), config);
            return crawler;
        }
    }
}