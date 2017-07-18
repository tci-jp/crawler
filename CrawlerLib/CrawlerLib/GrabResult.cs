namespace CrawlerLib
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
        public byte[] Content { get; set; }

        /// <summary>
        /// Http request status. If it's successful it must be HttpStatusCode.OK (even if real code is 201-299)
        /// </summary>
        public HttpStatusCode Status { get; set; }
    }
}