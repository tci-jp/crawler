// <copyright file="SearchTest.cs" company="DECTech.Tokyo">
// Copyright (c) DECTech.Tokyo. All rights reserved.
// </copyright>

namespace CrawlerLib.Tests
{
    using System.Collections.Async;
    using System.Threading;
    using System.Threading.Tasks;
    using Azure;
    using global::Azure.Storage;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Configuration.Json;
    using Xunit;
    using Xunit.Abstractions;

    public class SearchTest
    {
        private readonly AzureIndexedSearch blobSearcher;

        private readonly ITestOutputHelper output;

        public SearchTest(ITestOutputHelper output)
        {
            var config = new ConfigurationBuilder()
                .AddJsonFile("config.json")
                .Build();
            var searchServiceName = config["SearchServiceName"];
            var adminApiKey = config["SearchServiceAdminApiKey"];
            var textIndexName = config["TextSearchIndexName"];
            var metaIndexName = config["MetaSearchIndexName"];
            var azure = new DataStorage(config["CrawlerStorageConnectionString"]);

            this.output = output;
            blobSearcher = new AzureIndexedSearch(azure, searchServiceName, adminApiKey, textIndexName, metaIndexName);
        }

        [Fact]
        public async Task Main()
        {
            var result = blobSearcher.SearchByText("f", CancellationToken.None);
            await result.ForEachAsync(item => { output.WriteLine(item); });
        }
    }
}