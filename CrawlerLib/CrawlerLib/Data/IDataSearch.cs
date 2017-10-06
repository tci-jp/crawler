// <copyright file="IDataSearch.cs" company="DECTech.Tokyo">
// Copyright (c) DECTech.Tokyo. All rights reserved.
// </copyright>

namespace CrawlerLib.Data
{
    using System.Collections.Async;
    using System.Collections.Generic;
    using System.Threading;
    using JetBrains.Annotations;

    /// <summary>
    /// Searches in content and metadata.
    /// </summary>
    public interface IDataSearch
    {
        /// <summary>
        /// Search blobs by metadata
        /// </summary>
        /// <param name="query">Collection of operators combined together as AND.</param>
        /// <param name="cancellation">Search cancellation.</param>
        /// <returns>Collection of URIs which metadata has that values.</returns>
        [UsedImplicitly]
        IAsyncEnumerable<string> SearchByMeta(
            IEnumerable<SearchCondition> query,
            CancellationToken cancellation = default(CancellationToken));

        /// <summary>
        /// Search URLs content by free text
        /// </summary>
        /// <param name="text">Text to search.</param>
        /// <param name="cancellation">Search cancellation.</param>
        /// <returns>Collection of URIs which content has that text.</returns>
        [UsedImplicitly]
        IAsyncEnumerable<string> SearchByText(string text, CancellationToken cancellation = default(CancellationToken));
    }
}