// <copyright file="AzureIndexedSearch.cs" company="DECTech.Tokyo">
// Copyright (c) DECTech.Tokyo. All rights reserved.
// </copyright>

namespace CrawlerLib.Azure
{
    using System;
    using System.Collections.Async;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using Data;
    using global::Azure.Storage;
    using Microsoft.Azure.Search;
    using Microsoft.Azure.Search.Models;
    using Microsoft.WindowsAzure.Storage.Table;

    /// <inheritdoc cref="IBlobSearcher" />
    /// <summary>
    /// Azure Search implementation for blob searching
    /// </summary>
    public class AzureIndexedSearch : IBlobSearcher, IDisposable
    {
        private static readonly Dictionary<SearchCondition.Operator, string> OpMapping =
            new Dictionary<SearchCondition.Operator, string>
            {
                [SearchCondition.Operator.Equal] = "eq",
                [SearchCondition.Operator.NotEqual] = "neq"
            };

        private readonly ISearchIndexClient metaIndexClient;
        private readonly string metaIndexName;
        private readonly SearchServiceClient serviceClient;
        private readonly DataStorage storage;
        private readonly ISearchIndexClient textIndexClient;
        private readonly string textIndexName;

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureIndexedSearch" /> class.
        /// </summary>
        /// <param name="storage">Azure Storage Helper class.</param>
        /// <param name="searchServiceName">Name of search service in Azure.</param>
        /// <param name="apiKey">Search service access key.</param>
        /// <param name="textIndexName">Name of index for full text search.</param>
        /// <param name="metaIndexName">Name of index for metadata text search.</param>
        public AzureIndexedSearch(
            DataStorage storage,
            string searchServiceName,
            string apiKey,
            string textIndexName,
            string metaIndexName)
        {
            this.storage = storage;
            this.textIndexName = textIndexName;
            this.metaIndexName = metaIndexName;
            serviceClient = new SearchServiceClient(searchServiceName, new SearchCredentials(apiKey));
            textIndexClient = serviceClient.Indexes.GetClient(textIndexName);
            metaIndexClient = serviceClient.Indexes.GetClient(metaIndexName);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            metaIndexClient?.Dispose();
            serviceClient?.Dispose();
            textIndexClient?.Dispose();
        }

        /// <inheritdoc />
        public IAsyncEnumerable<string> SearchByMeta(
            IEnumerable<SearchCondition> query,
            CancellationToken cancellation)
        {
            var querystring = string.Join(" and ", query.Select(ToStr));

            var tablequery = new TableQuery<BlobMetadataDictionary>().Where(querystring);

            var result = storage.ExecuteQueryAsync(tablequery);

            return result.Select(i => i.Uri);
        }

        /// <inheritdoc />
        public IAsyncEnumerable<string> SearchByText(string text, CancellationToken cancellation)
        {
            var searchingParameters = new SearchParameters
            {
                Select = new[] { "metadata_storage_name" }
            };
            return new AsyncEnumerable<string>(async yield =>
            {
                var results =
                    await textIndexClient.Documents.SearchAsync<BlobContentIndexItem>(
                                              text,
                                              searchingParameters,
                                              cancellationToken: cancellation);
                foreach (var item in results.Results.Select(b => b.Document.MetadataStorageName))
                {
                    await yield.ReturnAsync(item);
                }
            });
        }

        private static string ToStr(SearchCondition s)
        {
            return TableQuery.GenerateFilterCondition(s.Name, OpMapping[s.Op], s.Value);
        }
    }
}