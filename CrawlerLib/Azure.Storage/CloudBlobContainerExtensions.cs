// <copyright file="CloudBlobContainerExtensions.cs" company="DECTech.Tokyo">
// Copyright (c) DECTech.Tokyo. All rights reserved.
// </copyright>

namespace Azure.Storage
{
    using System.Collections.Async;
    using System.Threading;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Blob;

    /// <summary>
    /// Extension methods for CloudBlobContainer
    /// </summary>
    public static class CloudBlobContainerExtensions
    {
        /// <summary>
        /// Asyncronuosly enumerates blobs in the container
        /// </summary>
        /// <param name="container">Container to enumerate.</param>
        /// <param name="prefix">Blobs name prefix.</param>
        /// <param name="useFlat">Enumerate as flat.</param>
        /// <param name="details">Blob info details.</param>
        /// <param name="maxResults">Number of blobs to enumerate or Null</param>
        /// <param name="token">Cancellation.</param>
        /// <returns>Asynchronuous enumerable with blobs info.</returns>
        public static IAsyncEnumerable<IListBlobItem> ListBlobsAsync(
            this CloudBlobContainer container,
            string prefix,
            bool useFlat,
            BlobListingDetails details,
            int? maxResults = null,
            CancellationToken token = default(CancellationToken))
        {
            var requestOption = new BlobRequestOptions();
            var context = new OperationContext();

            return new AsyncEnumerable<IListBlobItem>(
                async yield =>
                {
                    BlobResultSegment segment = null;
                    while ((segment = await container.ListBlobsSegmentedAsync(
                                          prefix,
                                          useFlat,
                                          details,
                                          maxResults,
                                          segment?.ContinuationToken,
                                          requestOption,
                                          context,
                                          token).ConfigureAwait(false)) != null)
                    {
                        foreach (var item in segment.Results)
                        {
                            token.ThrowIfCancellationRequested();
                            yield?.ReturnAsync(item);
                        }
                    }
                });
        }
    }
}