// <copyright file="ICommitableParserJob.cs" company="DECTech.Tokyo">
// Copyright (c) DECTech.Tokyo. All rights reserved.
// </copyright>

namespace CrawlerLib
{
    using System.Threading.Tasks;

    /// <summary>
    /// Parser job that can be confirmed.
    /// </summary>
    public interface ICommitableParserJob : IParserJob
    {
        /// <summary>
        /// Confirm job is processed
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task Commit();
    }
}