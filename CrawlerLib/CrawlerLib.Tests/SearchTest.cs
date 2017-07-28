namespace CrawlerLib.Tests
{
    using System.Collections.Async;
    using System.Configuration;
    using System.Threading;
    using System.Threading.Tasks;
    using Azure;
    using Xunit;
    using Xunit.Abstractions;

    public class SearchTest
    {
        public SearchTest(ITestOutputHelper output)
        {
            var searchServiceName = ConfigurationManager.AppSettings["SearchServiceName"];
            var adminApiKey = ConfigurationManager.AppSettings["SearchServiceAdminApiKey"];
            var indexName = ConfigurationManager.AppSettings["SearchIndexName"];
            this.output = output;
            blobSearcher = new AzureIndexedSearch(searchServiceName, adminApiKey, indexName);
        }

        private readonly ITestOutputHelper output;

        private readonly AzureIndexedSearch blobSearcher;

        [Fact]
        public async Task Main()
        {
            var result = await blobSearcher.SearchByText("f", CancellationToken.None);
            await result.ForEachAsync(item => { output.WriteLine(item); });
        }
    }
}