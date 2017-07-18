﻿namespace CrawlerLib
{
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// Grabs content by URI
    /// </summary>
    public abstract class HttpGrabber: IDisposable
    {
        protected HttpGrabber(Configuration config)
        {
        }

        /// <summary>
        /// Grabs content. Is threadsafe and reusable (does not need to instantiate HttpGrabber for each Grab call)
        /// </summary>
        /// <param name="uri">Uri to grab</param>
        /// <param name="referer">Browser referer. Can be null.</param>
        /// <returns>Grabed content</returns>
        public abstract Task<GrabResult> Grab(Uri uri, Uri referer);

        /// <inheritdoc />
        public void Dispose()
        {
        }
    }
}