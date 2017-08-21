// <copyright file="IUriState.cs" company="DECTech.Tokyo">
// Copyright (c) DECTech.Tokyo. All rights reserved.
// </copyright>

namespace CrawlerLib.Data
{
    /// <summary>
    /// Crawler state for specific URI.
    /// </summary>
    public interface IUriState
    {
        /// <summary>
        /// Gets crawling state as HTTP status code.
        /// </summary>
        int State { get; }

        /// <summary>
        /// Gets crawled URI.
        /// </summary>
        string Uri { get; }
    }
}