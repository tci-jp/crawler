// <copyright file="SimpleBlobSearcher.cs" company="DECTech.Tokyo">
// Copyright (c) DECTech.Tokyo. All rights reserved.
// </copyright>

namespace CrawlerLib.Azure
{
    using System;
    using System.Collections.Async;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading;
    using Data;
    using global::Azure.Storage;
    using JetBrains.Annotations;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Blob;

    /// <inheritdoc />
    /// <summary>
    /// Simple search listing all blobs and download them to search.
    /// </summary>
    [UsedImplicitly]
    public class SimpleBlobSearcher : IBlobSearcher
    {
        private readonly DataStorage azure;
        private readonly string containerName;
        private CloudBlobContainer container;

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleBlobSearcher" /> class.
        /// </summary>
        /// <param name="azure">Connected Azure Storage helper class.</param>
        /// <param name="containerName">Name of container with blobs.</param>
        public SimpleBlobSearcher(DataStorage azure, string containerName)
        {
            this.azure = azure;
            this.containerName = containerName;
        }

        /// <inheritdoc />
        public IAsyncEnumerable<string> SearchByMeta(
            IEnumerable<SearchCondition> query,
            CancellationToken cancellation)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public IAsyncEnumerable<string> SearchByText(string text, CancellationToken cancellation)
        {
            var condition = new AccessCondition();
            var options = new BlobRequestOptions();
            var context = new OperationContext();
            return new AsyncEnumerable<string>(async yield =>
            {
                if (container == null)
                {
                    container = await azure.GetBlobContainerAsync(containerName);
                }

                var blobs = container.ListBlobsAsync(null, true, BlobListingDetails.All)
                                      .Where(i => i is CloudBlockBlob).Cast<CloudBlockBlob>();
                await blobs.ForEachAsync(
                    async blob =>
                    {
                        cancellation.ThrowIfCancellationRequested();
                        yield.CancellationToken.ThrowIfCancellationRequested();
                        var str = await blob.DownloadTextAsync(
                                      Encoding.UTF8,
                                      condition,
                                      options,
                                      context,
                                      cancellation);
                        if (str.IndexOf(text, StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            await yield.ReturnAsync(blob.Name);
                        }
                    },
                    cancellation);
            });
        }
    }
}