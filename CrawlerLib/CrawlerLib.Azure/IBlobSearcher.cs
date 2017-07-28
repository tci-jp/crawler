﻿// <copyright file="IBlobSearcher.cs" company="DECTech.Tokyo">
// Copyright (c) DECTech.Tokyo. All rights reserved.
// </copyright>

namespace CrawlerLib.Azure
{
    using System.Collections.Async;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Blob searching interface.
    /// </summary>
    public interface IBlobSearcher
    {
        /// <summary>
        /// Returns async enumeration of blob names containing text
        /// </summary>
        /// <param name="text">Text to search.</param>
        /// <param name="cancellation">Search cancellation.</param>
        /// <returns>Async enumeration of blob names.</returns>
        Task<IAsyncEnumerable<string>> SearchByText(string text, CancellationToken cancellation);

        //Task<IAsyncEnumerable<string>> SearchByMeta(string query, CancellationToken cancellation);
    }
}