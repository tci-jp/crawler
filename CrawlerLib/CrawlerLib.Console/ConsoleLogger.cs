namespace CrawlerLib.Console
{
    using System;
    using Logger;

    public class ConsoleLogger : ILogger
    {
        public void Log(LogRecord log)
        {
            Console.WriteLine(log.ToString());
        }
    }
}