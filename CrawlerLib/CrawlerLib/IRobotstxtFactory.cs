// <copyright file="IRobotstxtFactory.cs" company="DECTech.Tokyo">
// Copyright (c) DECTech.Tokyo. All rights reserved.
// </copyright>
namespace CrawlerLib
{
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// Factory to download robots.txt by URL and process it.
    /// </summary>
    public interface IRobotstxtFactory
    {
        /// <summary>
        ///  Retrieve Robots object.
        /// </summary>
        /// <param name="uri">uri</param>
        /// <returns><see cref="Robots"/> instance if file is found. Or if not found instance of <see cref="NotFoundRobots"/>/></returns>
         Task<IRobots> RetrieveAsync(Uri uri);
    }
}
