// <copyright file="LoggerExtensions.cs" company="DECTech.Tokyo">
// Copyright (c) DECTech.Tokyo. All rights reserved.
// </copyright>

namespace CrawlerLib.Logger
{
    using System;
    using JetBrains.Annotations;

    /// <summary>
    /// Helper extensions for <see cref="ILogger"/> inteface.
    /// </summary>
    public static class LoggerExtensions
    {
        /// <summary>
        /// Sends basic debug text message.
        /// </summary>
        /// <param name="logger">Logger to send record.</param>
        /// <param name="message">Text message.</param>
        [UsedImplicitly]
        public static void Debug(this ILogger logger, string message)
        {
            logger?.Log(new LogRecord(Severity.Debug, message));
        }

        /// <summary>
        /// Sends basic error text message.
        /// </summary>
        /// <param name="logger">Logger to send record.</param>
        /// <param name="message">Text message.</param>
        [UsedImplicitly]
        public static void Error(this ILogger logger, string message)
        {
            logger?.Log(new LogRecord(Severity.Error, message));
        }

        /// <summary>
        /// Sends error message with exception
        /// </summary>
        /// <param name="logger">Logger to send record.</param>
        /// <param name="message">Text message.</param>
        /// <param name="exception">Exception.</param>
        [UsedImplicitly]
        public static void Error(this ILogger logger, string message, Exception exception)
        {
            logger?.Log(new LogRecord(Severity.Error, new { Message = message, Exception = exception }));
        }

        /// <summary>
        /// Sends error message with exception
        /// </summary>
        /// <param name="logger">Logger to send record.</param>
        /// <param name="exception">Exception.</param>
        [UsedImplicitly]
        public static void Error(this ILogger logger, Exception exception)
        {
            logger?.Log(new LogRecord(Severity.Error, exception));
        }

        /// <summary>
        /// Sends error message with exception
        /// </summary>
        /// <param name="logger">Logger to send record.</param>
        /// <param name="message">Text message.</param>
        [UsedImplicitly]
        public static void Trace(this ILogger logger, string message)
        {
            logger?.Log(new LogRecord(Severity.Trace, message));
        }
    }
}