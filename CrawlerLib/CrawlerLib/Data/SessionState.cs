// <copyright file="SessionState.cs" company="DECTech.Tokyo">
// Copyright (c) DECTech.Tokyo. All rights reserved.
// </copyright>

namespace CrawlerLib.Data
{
    /// <summary>
    /// Session execution state.
    /// </summary>
    public enum SessionState
    {
        /// <summary>
        /// Session is not started yet.
        /// </summary>
        NotStarted = 0,

        /// <summary>
        /// Session is still in processing.
        /// </summary>
        InProcess = 1,

        /// <summary>
        /// Session is finished successfully.
        /// </summary>
        Done = 3,

        /// <summary>
        /// Session processing failed because of some server error.
        /// </summary>
        Error = -1,

        /// <summary>
        /// Session or page processing is cancelled.
        /// </summary>
        Cancelled = 2
    }
}