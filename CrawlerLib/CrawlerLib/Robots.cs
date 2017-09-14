// <copyright file="Robots.cs" company="DECTech.Tokyo">
// Copyright (c) DECTech.Tokyo. All rights reserved.
// </copyright>

namespace CrawlerLib
{
    /// <summary>
    /// Checks if path is allowed for crawling by web site's robots.txt
    /// </summary>
    public class Robots : IRobots
    {
        private readonly string userAgent;
        private readonly RobotsTxt.Robots robotstxt;

        /// <summary>
        /// Initializes a new instance of the <see cref="Robots"/> class.
        /// </summary>
        /// <param name="userAgent">User agent used for grabbing.</param>
        /// <param name="robotstxt">Content of rebotstxt.</param>
        public Robots(string userAgent, string robotstxt)
        {
            this.userAgent = userAgent;
            this.robotstxt = new RobotsTxt.Robots(robotstxt);
        }

        /// <inheritdoc />
        public bool IsPathAllowed(string uriPathAndQuery)
        {
            return robotstxt.IsPathAllowed(userAgent, uriPathAndQuery);
        }
    }
}