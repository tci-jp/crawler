// <copyright file="NotFoundRobots.cs" company="DECTech.Tokyo">
// Copyright (c) DECTech.Tokyo. All rights reserved.
// </copyright>
namespace CrawlerLib
{
    /// <summary>
    /// NotFoundRobots
    /// </summary>
    public class NotFoundRobots : IRobots
    {
        /// <inheritdoc/>
        public bool IsPathAllowed(string uriPathAndQuery)
        {
            return true;
        }
    }
}
