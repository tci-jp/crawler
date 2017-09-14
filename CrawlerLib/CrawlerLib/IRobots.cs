// <copyright file="IRobots.cs" company="DECTech.Tokyo">
// Copyright (c) DECTech.Tokyo. All rights reserved.
// </copyright>

namespace CrawlerLib
{
    /// <summary>
    /// Checks if path is allowed for crawling by web site's robots.txt
    /// </summary>
    public interface IRobots
    {
        /// <summary>
        /// Checks if path is allowed for crawling.
        /// </summary>
        /// <param name="uriPathAndQuery">Path to check.</param>
        /// <returns>True if allowed.</returns>
        bool IsPathAllowed(string uriPathAndQuery);
    }
}