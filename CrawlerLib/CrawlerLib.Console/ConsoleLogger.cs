// <copyright file="ConsoleLogger.cs" company="DECTech.Tokyo">
// Copyright (c) DECTech.Tokyo. All rights reserved.
// </copyright>

namespace CrawlerLib.Console
{
    using System;
    using Logger;

    /// <summary>
    /// Console implementation of logger.
    /// </summary>
    public class ConsoleLogger : ILogger
    {
        /// <inheritdoc />
        public void Log(LogRecord log)
        {
            Console.WriteLine(log.ToString());
        }
    }
}