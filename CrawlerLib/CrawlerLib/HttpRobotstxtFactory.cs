// <copyright file="HttpRobotstxtFactory.cs" company="DECTech.Tokyo">
// Copyright (c) DECTech.Tokyo. All rights reserved.
// </copyright>
namespace CrawlerLib
{
    using System;
    using System.Collections.Concurrent;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;

    /// <summary>
    /// HttpRobotstxtFactory
    /// </summary>
    public class HttpRobotstxtFactory : IRobotstxtFactory, IDisposable
    {
        private static readonly NotFoundRobots NotFoundRobots = new NotFoundRobots();
        private readonly string agent;
        private HttpClient client;

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpRobotstxtFactory"/> class.x
        /// </summary>
        /// <param name="agentParam">agent for HttpRobotstxtFactory</param>
        public HttpRobotstxtFactory(string agentParam)
        {
            agent = agentParam;

            client = new HttpClient();
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            client?.Dispose();
        }

        /// <inheritdoc/>
        public async Task<IRobots> RetrieveAsync(Uri uri)
        {
            using (var result = await client.GetAsync(uri))
            {
                if (result.StatusCode != HttpStatusCode.NotFound)
                {
                    string content = await result.Content.ReadAsStringAsync();
                    return new Robots(agent, content);
                }
            }

            return NotFoundRobots;
        }
    }
}
