// <copyright file="FakeRobotstxtFactory.cs" company="DECTech.Tokyo">
// Copyright (c) DECTech.Tokyo. All rights reserved.
// </copyright>

namespace CrawlerLib.Tests
{
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// FakeRobotstxtFactory
    /// </summary>
    public class FakeRobotstxtFactory : IRobotstxtFactory
    {
        private static readonly NotFoundRobots NotFoundRobots = new NotFoundRobots();

        /// <inheritdoc/>
        public Task<IRobots> RetrieveAsync(Uri uri)
        {
            return Task.FromResult<IRobots>(NotFoundRobots);
        }
    }
}
