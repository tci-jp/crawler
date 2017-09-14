// <copyright file="IRobotstxtFactory.cs" company="DECTech.Tokyo">
// Copyright (c) DECTech.Tokyo. All rights reserved.
// </copyright>
namespace CrawlerLib
{
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// interface IRobotstxtFactory
    /// </summary>
    public interface IRobotstxtFactory
    {
        /// <summary>
        ///  Retrieve
        /// </summary>
        /// <param name="uri">uri</param>
        /// <returns>IRobots</returns>
         Task<IRobots> RetrieveAsync(Uri uri);
    }
}
