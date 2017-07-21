// <copyright file="ILogger.cs" company="DECTech.Tokyo">
// Copyright (c) DECTech.Tokyo. All rights reserved.
// </copyright>

namespace CrawlerLib.Logger
{
    /// <summary>
    /// General interface for logging.
    /// </summary>
    public interface ILogger
    {
        /// <summary>
        /// Send record to log.
        /// </summary>
        /// <param name="log">Log record.</param>
        void Log(LogRecord log);
    }
}