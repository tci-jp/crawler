// <copyright file="SimpleBlobSearcher.cs" company="DECTech.Tokyo">
// Copyright (c) DECTech.Tokyo. All rights reserved.
// </copyright>

namespace CrawlerLib.Azure
{
    using System;
    using System.Collections.Async;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Data;
    using global::Azure.Storage;
    using JetBrains.Annotations;
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
        public Task<IAsyncEnumerable<string>> SearchByMeta(
            IEnumerable<SearchCondition> query,
            CancellationToken cancellation)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public async Task<IAsyncEnumerable<string>> SearchByText(string text, CancellationToken cancellation)
        {
            if (container == null)
            {
                container = await azure.GetBlobContainer(containerName);
            }

            var blobs = container.ListBlobs(null, true, BlobListingDetails.All)
                                  .OfType<CloudBlockBlob>();
            return new AsyncEnumerable<string>(async yield =>
            {
                foreach (var blob in blobs)
                {
                    cancellation.ThrowIfCancellationRequested();
                    yield.CancellationToken.ThrowIfCancellationRequested();
                    var str = await blob.DownloadTextAsync(yield.CancellationToken);
                    if (str.IndexOf(text, StringComparison.InvariantCultureIgnoreCase) >= 0)
                    {
                        await yield.ReturnAsync(blob.Name).ConfigureAwait(false);
                    }
                }
            });
        }
    }
}