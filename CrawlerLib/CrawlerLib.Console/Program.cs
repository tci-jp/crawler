// <copyright file="Program.cs" company="DECTech.Tokyo">
// Copyright (c) DECTech.Tokyo. All rights reserved.
// </copyright>

namespace CrawlerLib.Console
{
    using System;
    using System.Threading;
    using Grabbers;
    using JetBrains.Annotations;
    using Logger;

    [UsedImplicitly]
    public class Program
    {
        private static CancellationTokenSource cancellation;

        public static void Main(string[] args)
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
                config.HttpGrabber = new WebDriverHttpGrabber(config);

                var crawler = new Crawler(config);
                var crawledUriNumbder = 0;
                crawler.UriCrawled += (sender, u) => { Interlocked.Increment(ref crawledUriNumbder); };

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