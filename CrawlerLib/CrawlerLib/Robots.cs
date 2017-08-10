// <copyright file="Robots.cs" company="DECTech.Tokyo">
// Copyright (c) DECTech.Tokyo. All rights reserved.
// </copyright>

namespace CrawlerLib
{
    /// <summary>
    /// Checks if path is allowed for crawling by web site's robots.txt
    /// </summary>
    public class Robots
    {
        private string userAgent;
        private object robotstxt;

        /// <summary>
        /// Initializes a new instance of the <see cref="Robots"/> class.
        /// </summary>
        /// <param name="userAgent">User agent used for grabbing.</param>
        /// <param name="robotstxt">Content of rebotstxt.</param>
        public Robots(string userAgent, object robotstxt)
        {
            this.userAgent = userAgent;
            this.robotstxt = robotstxt;
        }

        /// <summary>
        /// Checks if path is allowed for crawling.
        /// </summary>
        /// <param name="uriPathAndQuery">Path to check.</param>
        /// <returns>True if allowed.</returns>
        public bool IsPathAllowed(string uriPathAndQuery)
        {
            return true;
        }
    }
}