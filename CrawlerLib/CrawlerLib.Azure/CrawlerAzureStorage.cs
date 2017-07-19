namespace CrawlerLib.Azure
{
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using System.Threading.Tasks;
    using Data;
    using global::Azure.Storage;
    using JetBrains.Annotations;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Azure.Search;
    using Microsoft.Azure.Search.Models;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Auth;
    using Microsoft.WindowsAzure.Storage.Blob;
    using System.Linq;
    using CrawlerLib.Logger;
    using System;
    using System.Threading;

    [UsedImplicitly]
    public class CrawlerAzureStorage : ICrawlerStorage
    {
        private readonly DataStorage storage;
        private ILogger logger;
        public CrawlerAzureStorage(DataStorage storage)
        {
            this.storage = storage;
        }



        public async Task DumpPage(string uri, Stream content)
        {
            var container = await storage.GetBlobContainer("pages");
            var blob = container.GetBlockBlobReference(uri);
            await blob.UploadFromStreamAsync(content);

            await storage.InsertOrReplaceAsync(new CrawlRecord(uri)
            {
                Status = "OK"
            });
        }

        public Task<string> CreateSession(IEnumerable<string> rootUris)
        {
            throw new System.NotImplementedException();
        }

        public Task<IEnumerable<SessionInfo>> GetAllSessions()
        {
            throw new System.NotImplementedException();
        }

        public Task AddPageReferer(string sessionId, string uri, string referer)
        {
            throw new System.NotImplementedException();
        }

        public Task StorePageError(string sessionId, string uri, HttpStatusCode code)
        {
            throw new System.NotImplementedException();
        }

        public Task<IEnumerable<string>> GetSessionUris(string sessionId)
        {
            throw new System.NotImplementedException();
        }

        public Task<Stream> GetUriContet(string uri)
        {
            throw new System.NotImplementedException();
        }

        public Task<IEnumerable<string>> GetReferers(string sessionId, string uri)
        {
            throw new System.NotImplementedException();
        }

        public Task<IEnumerable<string>> SearchText(string text)
        {
            //get index configuration
            IConfigurationBuilder builder = new ConfigurationBuilder().AddJsonFile("indexconfig.json");
            IConfigurationRoot configuration = builder.Build();

            //get search service info
            string searchServiceName = configuration["SearchServiceName"];
            string adminApiKey = configuration["SearchServiceAdminApiKey"];
            string queryApiKey = configuration["SearchServiceQueryApiKey"];
            string indexName = configuration["SearchIndexName"];

            //create search service client
            SearchServiceClient serviceClient = new SearchServiceClient(searchServiceName, new SearchCredentials(adminApiKey));

            //delete index if exists
            if (serviceClient.Indexes.Exists(indexName))
            {
                serviceClient.Indexes.Delete(indexName);
            }

            //create index 
            var definition = new Index()
            {
                Name = indexName,
                Fields = FieldBuilder.BuildForType<Blob>()
            };
            serviceClient.Indexes.Create(definition);

            ISearchIndexClient indexClient = serviceClient.Indexes.GetClient(indexName);

            //create documents
            UploadDocumentsFromBlob(indexClient, configuration).Wait();

            ISearchIndexClient indexClientForQueries = new SearchIndexClient(searchServiceName, indexName, new SearchCredentials(queryApiKey));


            RunQueries(indexClientForQueries, text);
        }

        private static async Task UploadDocumentsFromBlob(ISearchIndexClient indexClient, IConfigurationRoot configuration)
        {

            var accountName = configuration["BlobStorageAccountName"];
            var accountKey = configuration["BlobStorageAccountKey"];
            var storageAccount = new CloudStorageAccount(new StorageCredentials(accountName, accountKey), true);
            var blobClient = storageAccount.CreateCloudBlobClient();

            BlobContinuationToken continuationToken = null;
            BlobContinuationToken containerContinuationToken = null;
            List<string> containerNames = new List<string>();

            var fetchContainersTask = Task.Run<ContainerResultSegment>(async () =>
            {
                return await blobClient.ListContainersSegmentedAsync("", ContainerListingDetails.All, 100, containerContinuationToken, null, null);
            });
            var fetchContainersTaskResult = fetchContainersTask.Result;
            containerContinuationToken = fetchContainersTaskResult.ContinuationToken;
            var containers = fetchContainersTaskResult.Results.ToList();

            foreach (var container in containers)
            {
                Console.WriteLine("Listing blobs from '" + container.Name + "' container and uploading them in search service.");
                //long totalBlobsUploaded = 0;
                continuationToken = null;

                var fetchBlobsTask = Task.Run<BlobResultSegment>(async () =>
                {
                    return await container.ListBlobsSegmentedAsync("", true, BlobListingDetails.All, 100, continuationToken, null, null);
                });
                var blobListingResult = fetchBlobsTask.Result;
                continuationToken = blobListingResult.ContinuationToken;
                var blobsList = blobListingResult.Results.ToList();
                if (blobsList.Count > 0)
                {
                    List<Blob> documentsList = new List<Blob>();

                    var pageHtml = "";
                    var count = 0;
                    foreach (var blob in blobsList)
                    {
                        var blobName = blob.Uri.AbsoluteUri.Substring(blob.Uri.AbsoluteUri.LastIndexOf('/') + 1);
                        CloudBlobContainer containerInstance = blobClient.GetContainerReference(container.Name);
                        CloudBlob blobInstance = containerInstance.GetBlobReference(blobName);

                        using (Stream stream = await blobInstance.OpenReadAsync())
                        {
                            using (StreamReader reader = new StreamReader(stream))
                            {
                                pageHtml = await reader.ReadToEndAsync();
                            }
                        }

                        //adding new document to list
                        Blob page = new Blob()
                        {
                            BlobUrl = "blob" + count,
                            Html = pageHtml
                        };
                        count++;
                        documentsList.Add(page);
                    }

                    var batch = IndexBatch.Upload(documentsList);

                    try
                    {
                        indexClient.Documents.Index(batch);
                    }
                    catch (IndexBatchException e)
                    {
                        // Sometimes when your Search service is under load, indexing will fail for some of the documents in
                        // the batch. Depending on your application, you can take compensating actions like delaying and
                        // retrying. For this simple demo, we just log the failed document keys and continue.
                        Console.WriteLine(
                            "Failed to index some of the documents: {0}",
                            String.Join(", ", e.IndexingResults.Where(r => !r.Succeeded).Select(r => r.Key)));
                    }

                    Console.WriteLine("Waiting for documents to be indexed...\n");
                    //Thread.Sleep(2000);
                }
            }
        }

        public async Task StorePageError(string uri, HttpStatusCode code)
        {
            await storage.InsertOrReplaceAsync(new CrawlRecord(uri)
            {
                Status = code.ToString()
            });
        }
    }
}