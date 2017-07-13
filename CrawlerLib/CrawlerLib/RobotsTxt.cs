// <copyright file="RobotsTxt.cs" company="DECTech.Tokyo">
// Copyright (c) DECTech.Tokyo. All rights reserved.
// </copyright>

namespace CrawlerLib
{
    public class RobotsTxt
    {
        public static readonly RobotsTxt DefaultInstance = new RobotsTxt();
        private string robotstxt;

        private RobotsTxt()
        {
        }

        public RobotsTxt(string robotstxt)
        {
            this.robotstxt = robotstxt;
        }
    }
}