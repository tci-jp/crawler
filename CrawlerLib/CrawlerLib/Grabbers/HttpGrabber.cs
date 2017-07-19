// <copyright file="HttpGrabber.cs" company="DECTech.Tokyo">
// Copyright (c) DECTech.Tokyo. All rights reserved.
// </copyright>

namespace CrawlerLib.Grabbers
{
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// Grabs content by URI
    /// </summary>
    public abstract class HttpGrabber : IDisposable
    {
        protected Configuration Config { get; }

        protected HttpGrabber(Configuration config)
        {
            Config = config;
        }

        /// <summary>
        /// Grabs content. Is threadsafe and reusable (does not need to instantiate HttpGrabber for each Grab call)
        /// </summary>
        /// <param name="uri">Uri to grab</param>
        /// <param name="referer">Browser referer. Can be null.</param>
        /// <returns>Grabed content</returns>
        public abstract Task<GrabResult> Grab(Uri uri, Uri referer);

        /// <inheritdoc />
        public virtual void Dispose()
        {
        }
    }
}