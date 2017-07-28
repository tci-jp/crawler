// <copyright file="BlobSearcher.cs" company="DECTech.Tokyo">
// Copyright (c) DECTech.Tokyo. All rights reserved.
// </copyright>

namespace CrawlerLib.Azure
{
    using System;
    using System.Collections.Async;
    using System.Collections.Generic;
    using System.Configuration;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using global::Azure.Storage;
    using Microsoft.Azure.Search;
    using Microsoft.Azure.Search.Models;
    using Microsoft.WindowsAzure.Storage.Blob;

    /// <inheritdoc />
    public class AzureIndexedSearch : IBlobSearcher
    {
        private readonly ISearchIndexClient indexClient;
        private readonly SearchParameters searchingParameters;

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureIndexedSearch"/> class.
        /// </summary>
        /// <param name="searchServiceName"></param>
        /// <param name="adminApiKey"></param>
        /// <param name="indexName"></param>
        public AzureIndexedSearch(string searchServiceName, string adminApiKey, string indexName)
        {
            // create search service client
            var serviceClient = new SearchServiceClient(searchServiceName, new SearchCredentials(adminApiKey));
            indexClient = serviceClient.Indexes.GetClient(indexName);
            searchingParameters =
                new SearchParameters()
                {
                    Select = new[] { "metadata_storage_name" }
                };
        }

        /// <inheritdoc />
        public async Task<IAsyncEnumerable<string>> SearchByText(string text, CancellationToken cancellation)
        {
           // search for phrase (full-text) in documents
            var results = await indexClient.Documents.SearchAsync<Blob>(text, searchingParameters, cancellationToken: cancellation);

            return results.Results.Select(b => b.Document.MetadataStorageName).ToAsyncEnumerable();
        }
    }
}