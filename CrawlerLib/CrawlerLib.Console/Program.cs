// <copyright file="Program.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace CrawlerLib.Console
{
    using System;
    using System.Threading;
    using Data;
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

                var config = new Configuration
                {
                    Logger = logger,
                    CancellationToken = cancellation.Token,
                    HostDepth = 1,
                    Depth = 3
                };
                var crawler = new Crawler(config);
                var crawledUriNumbder = 0;
                crawler.UriCrawled += u => { Interlocked.Increment(ref crawledUriNumbder); };

                try
                {
                    crawler.Incite(new Uri(uri)).Wait();
                    Console.WriteLine($"Done! {crawledUriNumbder} URIs processed.");
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
