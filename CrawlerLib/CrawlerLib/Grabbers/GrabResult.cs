// <copyright file="GrabResult.cs" company="DECTech.Tokyo">
// Copyright (c) DECTech.Tokyo. All rights reserved.
// </copyright>

namespace CrawlerLib.Grabbers
{
    using System.Net;

    /// <summary>
    /// Result of page grabbing.
    /// </summary>
    public class GrabResult
    {
        /// <summary>
        /// Grabbed content
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// Http request status. If it's successful it must be HttpStatusCode.OK (even if real code is 201-299)
        /// </summary>
        public HttpStatusCode Status { get; set; }
    }
}