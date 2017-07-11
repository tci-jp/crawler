namespace CrawlerLib.Console
{
    using System;
    using Logger;

    public class ConsoleLogger : ILogger
    {
        public void Log(Logger.LogRecord log)
        {
            Console.WriteLine(log.ToString());
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var logger = new ConsoleLogger();
            while (true)
            {
                Console.Write("> ");
                var uri = Console.ReadLine();

                var storage = new DummyStorage();
                var config = new Configuration
                {
                    Logger = logger,
                    Storage = storage
                };

                var crawler = new Crawler(config);

                try
                {
                    crawler.Incite(new Uri(uri)).Wait();
                    Console.WriteLine($"Done! {storage.DumpedPagesNumber} URIs processed.");
                }
                catch (Exception ex)
                {
                    logger.Error(ex);
                }
            }
        }
    }
}
