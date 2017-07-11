namespace CrawlerLib.Tests
{
    using System;
    using System.Threading.Tasks;
    using Xunit;
    using Xunit.Abstractions;

    public class TestCrawler
    {
        private readonly Crawler crawler;
        private readonly DummyStorage storage;

        private readonly ITestOutputHelper output;

        public TestCrawler(ITestOutputHelper output)
        {
            this.output = output;
            crawler = new Crawler();
            storage = crawler.Config.Storage as DummyStorage;
        }

        [Theory]
        [InlineData(3, 1, "http://www.dectech.tokyo")]
        [InlineData(3, 0, "http://schema.org")]
        public async Task TestInciteStability(int depth, int hostDepth, string url)
        {
            crawler.Config.HostDepth = hostDepth;
            crawler.Config.Depth = depth;

            await crawler.Incite(new Uri(url));
            foreach (var line in storage.DumpedPages)
            {
                output.WriteLine(line.Uri);
            }

            output.WriteLine(storage.DumpedPagesNumber.ToString());
        }
    }
}
