namespace CrawlerLib.Console
{
    using System;
    using System.Threading;
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
        private static CancellationTokenSource cancellation;

        static void Main(string[] args)
        {
            var logger = new ConsoleLogger();

            while (true)
            {
                Console.Write("> ");
                var uri = Console.ReadLine();
                if (uri == null)
                {
                    break;
                }

                Console.CancelKeyPress += Console_CancelKeyPress;

                cancellation = new CancellationTokenSource();

                var storage = new DummyStorage();
                var config = new Configuration
                {
                    Logger = logger,
                    Storage = storage,
                    CancellationToken = cancellation.Token,
                    HostDepth = 0,
                    Depth = 1
                };
                var crawler = new Crawler(config);

                try
                {
                    crawler.Incite(new Uri(uri)).Wait();
                    Console.WriteLine($"Done! {storage.DumpedPagesNumber} URIs processed.");
                    Console.CancelKeyPress -= Console_CancelKeyPress;
                }
                catch (Exception ex)
                {
                    logger.Error(ex);
                    Console.WriteLine();
                }

            }

            Console.WriteLine("\r\nExit");
        }

        private static void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            cancellation.Cancel();
        }
    }
}
