// <copyright file="IParserJobsQueue.cs" company="DECTech.Tokyo">
// Copyright (c) DECTech.Tokyo. All rights reserved.
// </copyright>

namespace CrawlerLib
{
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Queue to enqueue or dequeue ParserJobs.
    /// </summary>
    public interface IParserJobsQueue
    {
        /// <summary>
        /// Enques parser job for future processing.
        /// </summary>
        /// <param name="job">Job for parsing.</param>
        /// <param name="cancellation">Cancellation for enqueue.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task EnqueueAsync(IParserJob job, CancellationToken cancellation);

        /// <summary>
        /// Dequeues parser job. Blocks till job get available or dequeue cancelled.
        /// </summary>
        /// <param name="cancellation">Cancellation for dequeue.</param>
        /// <returns>Job for parser.</returns>
        Task<ICommitableParserJob> DequeueAsync(CancellationToken cancellation);

        /// <summary>
        /// Wait till queue get empty
        /// </summary>
        /// <param name="sessionid">Session Id.</param>
        /// <param name="cancellation">Wait cancellation.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task WaitForSession(string sessionid, CancellationToken cancellation);
    }
}