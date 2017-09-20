// <copyright file="IBlobSearcher.cs" company="DECTech.Tokyo">
// Copyright (c) DECTech.Tokyo. All rights reserved.
// </copyright>

namespace CrawlerLib.Data
{
    using System.Collections.Async;
    using System.Collections.Generic;
    using System.Threading;

    /// <summary>
    /// Blob searching interface.
    /// </summary>
    public interface IBlobSearcher
    {
        /// <summary>
        /// Search blobs by metadata
        /// </summary>
        /// <param name="query">Collection of operators combined together as AND.</param>
        /// <param name="cancellation">Search cancellation.</param>
        /// <returns>Async enumeration of blob names.</returns>
        IAsyncEnumerable<string> SearchByMeta(IEnumerable<SearchCondition> query, CancellationToken cancellation);

        /// <summary>
        /// Returns async enumeration of blob names containing text
        /// </summary>
        /// <param name="text">Text to search.</param>
        /// <param name="cancellation">Search cancellation.</param>
        /// <returns>Async enumeration of blob names.</returns>
        IAsyncEnumerable<string> SearchByText(string text, CancellationToken cancellation);
    }
}