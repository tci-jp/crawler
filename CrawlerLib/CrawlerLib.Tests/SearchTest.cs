// <copyright file="SearchTest.cs" company="DECTech.Tokyo">
// Copyright (c) DECTech.Tokyo. All rights reserved.
// </copyright>

namespace CrawlerLib.Tests
{
    using System.Collections.Async;
    using System.Configuration;
    using System.Threading;
    using System.Threading.Tasks;
    using Azure;
    using global::Azure.Storage;
    using Xunit;
    using Xunit.Abstractions;

    public class SearchTest
    {
        private readonly AzureIndexedSearch blobSearcher;

        private readonly ITestOutputHelper output;

        public SearchTest(ITestOutputHelper output)
        {
            var searchServiceName = ConfigurationManager.AppSettings["SearchServiceName"];
            var adminApiKey = ConfigurationManager.AppSettings["SearchServiceAdminApiKey"];
            var textIndexName = ConfigurationManager.AppSettings["TextSearchIndexName"];
            var metaIndexName = ConfigurationManager.AppSettings["MetaSearchIndexName"];
            var azure = new DataStorage(ConfigurationManager.AppSettings["CrawlerStorageConnectionString"]);

            this.output = output;
            blobSearcher = new AzureIndexedSearch(azure, searchServiceName, adminApiKey, textIndexName, metaIndexName);
        }

        [Fact]
        public async Task Main()
        {
            var result = await blobSearcher.SearchByText("f", CancellationToken.None);
            await result.ForEachAsync(item => { output.WriteLine(item); });
        }
    }
}