// <copyright file="ICommitableParserJob.cs" company="DECTech.Tokyo">
// Copyright (c) DECTech.Tokyo. All rights reserved.
// </copyright>

namespace CrawlerLib.Queue
{
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Parser job that can be confirmed.
    /// </summary>
    public interface ICommitableParserJob : IParserJob
    {
        /// <summary>
        /// Confirm job is processed
        /// </summary>
        /// <param name="canncellation">Commit cancellation.</param>
        /// <param name="status">HTTP status.</param>
        /// <param name="message">Error message</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task Commit(CancellationToken canncellation, int status, string message = null);
    }
}