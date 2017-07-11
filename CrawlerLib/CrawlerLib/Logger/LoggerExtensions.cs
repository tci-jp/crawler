namespace CrawlerLib.Logger
{
    using System;

    public static class LoggerExtensions
    {
        public static void Debug(this ILogger logger, string message)
        {
            logger.Log(new LogRecord(Severity.Debug, message));
        }

        public static void Error(this ILogger logger, string message)
        {
            logger.Log(new LogRecord(Severity.Error, message));
        }

        public static void Trace(this ILogger logger, string message)
        {
            logger.Log(new LogRecord(Severity.Trace, message));
        }

        public static void Error(this ILogger logger, string message, Exception exception)
        {
            logger.Log(new LogRecord(Severity.Error, message, exception));
        }

        public static void Error(this ILogger logger, Exception exception)
        {
            logger.Log(new LogRecord(Severity.Error, null, exception));
        }
    }
}