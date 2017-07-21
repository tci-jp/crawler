// <copyright file="HttpGrabber.cs" company="DECTech.Tokyo">
// Copyright (c) DECTech.Tokyo. All rights reserved.
// </copyright>

namespace CrawlerLib.Grabbers
{
    using System;
    using System.Threading.Tasks;

    /// <inheritdoc />
    /// <summary>
    /// Grabs content by URI
    /// </summary>
    public abstract class HttpGrabber : IDisposable
    {
        /// <inheritdoc />
        protected HttpGrabber(Configuration config)
        {
            Config = config;
        }

        /// <summary>
        /// Gets crawler Configuration
        /// </summary>
        protected Configuration Config { get; }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Grabs content. Is threadsafe and reusable (does not need to instantiate HttpGrabber for each Grab call)
        /// </summary>
        /// <param name="uri">Uri to grab</param>
        /// <param name="referer">Browser referer. Can be null.</param>
        /// <returns>Grabed content</returns>
        public abstract Task<GrabResult> Grab(Uri uri, Uri referer);

        /// <summary>
        /// Disposer.
        /// </summary>
        /// <param name="disposing">True if disposing.</param>
        protected virtual void Dispose(bool disposing)
        {
        }
    }
}