namespace CrawlerLib.Tests
{
    using System;
    using System.Threading.Tasks;
    using Xunit;
    using Xunit.Abstractions;

    public class TestCrawler
    {
        private readonly ITestOutputHelper output;
        private readonly DummyStorage storage;

        private readonly Configuration config = new Configuration();

        private Crawler crawler;

        public TestCrawler(ITestOutputHelper output)
        {
            this.output = output;
            storage = config.Storage as DummyStorage;
        }

        private Crawler Crawler => crawler ?? (crawler = new Crawler(config));

        [Theory]
        [InlineData(3, 0, "http://www.dectech.tokyo")]
        public async Task TestInciteStability(int depth, int hostDepth, string url)
        {
            config.HostDepth = hostDepth;
            config.Depth = depth;

            await Crawler.Incite(new Uri(url));

            foreach (var line in storage.DumpedPages)
            {
                output.WriteLine(line.Uri);
            }

            output.WriteLine(storage.DumpedPagesNumber.ToString());
        }
    }
}