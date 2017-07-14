// <copyright file="LoggerExtensions.cs" company="DECTech.Tokyo">
// Copyright (c) DECTech.Tokyo. All rights reserved.
// </copyright>

namespace CrawlerLib.Logger
{
    using System;
    using JetBrains.Annotations;

    public static class LoggerExtensions
    {
        [UsedImplicitly]
        public static void Debug(this ILogger logger, string message)
        {
            logger?.Log(new LogRecord(Severity.Debug, message));
        }

        [UsedImplicitly]
        public static void Error(this ILogger logger, string message)
        {
            logger?.Log(new LogRecord(Severity.Error, message));
        }

        [UsedImplicitly]
        public static void Error(this ILogger logger, string message, Exception exception)
        {
            logger?.Log(new LogRecord(Severity.Error, message, exception));
        }

        [UsedImplicitly]
        public static void Error(this ILogger logger, Exception exception)
        {
            logger?.Log(new LogRecord(Severity.Error, null, exception));
        }

        [UsedImplicitly]
        public static void Trace(this ILogger logger, string message)
        {
            logger?.Log(new LogRecord(Severity.Trace, message));
        }
    }
}