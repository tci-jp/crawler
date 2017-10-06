// <copyright file="IMetadataStorage.cs" company="DECTech.Tokyo">
// Copyright (c) DECTech.Tokyo. All rights reserved.
// </copyright>

namespace CrawlerLib.Data
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Storage for metadata.
    /// </summary>
    public interface IMetadataStorage
    {
        /// <summary>
        /// Saved URI metadata into the storage
        /// </summary>
        /// <param name="ownerId">Crawling owner id</param>
        /// <param name="sessionId">Crawling session id.</param>
        /// <param name="uri">Page URI.</param>
        /// <param name="metadata">Collection of metadata as field/value pairs.</param>
        /// <param name="cancellation">Operation cancellation.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task DumpUriMetadataAsync(
           string ownerId,
           string sessionId,
           string uri,
           IEnumerable<KeyValuePair<string, string>> metadata,
           CancellationToken cancellation);
    }
}