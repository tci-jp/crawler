// <copyright file="Program.cs" company="DECTech.Tokyo">
// Copyright (c) DECTech.Tokyo. All rights reserved.
// </copyright>

namespace CrawlerWebJob
{
    using System;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using Azure.Storage;
    using CrawlerLib;
    using CrawlerLib.Azure;
    using CrawlerLib.Data;
    using CrawlerLib.Grabbers;
    using CrawlerLib.Logger;
    using CrawlerLib.Metadata;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Extensions.Configuration;

    public class Program
    {
        private static IConfiguration configuration;
        private static Crawler crawler;
        private static WebJobsQueue queue;

        public static async Task ProcessMessage(
            [QueueTrigger("%CrawlerJobsQueueName%")] string message,
            TextWriter log,
            CancellationToken cancellation)
        {
            var job = queue.ConvertMessage(message);
            await crawler.InciteJob(job, cancellation);
        }

        private static void Main(string[] args)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", true, true)
                .AddEnvironmentVariables();
            configuration = builder.Build();

            var storage = new DataStorage(configuration["CrawlerStorageConnectionString"]);

            var azureIndexedSearch = new AzureIndexedSearch(
                storage,
                configuration["SearchServiceName"],
                configuration["SearchServiceAdminApiKey"],
                configuration["TextSearchIndexName"],
                configuration["MetaSearchIndexName"]);

            var crawlerStorage = new AzureCrawlerStorage(storage, azureIndexedSearch);
            var dataLake = new DECAds.DataLakePlugin.Logger(configuration["DataLakeConnectionString"]);

            queue = new WebJobsQueue(storage, configuration["CrawlerJobsQueueName"], TimeSpan.FromMinutes(10));

            var config = new Configuration
            {
                Storage = crawlerStorage,
                MetadataStorages = new IMetadataStorage[] { crawlerStorage, new DataLakeMetadataStorage(dataLake) },
                Queue = queue,
                Logger = new ConsoleLogger(),
                Depth = 0,
                HostDepth = 0,
                MetadataExtractors = new IMetadataExtractor[]
                                                  {
                                                      new RdfaMetadataExtractor(),
                                                      new MicrodataMetadataExtractor(),
                                                      new JsonMetadataExtractor()
                                                  }
            };

            config.HttpGrabber = new WebDriverHttpGrabber(config);

            crawler = new Crawler(config);

            var jobconfig = new JobHostConfiguration
            {
                DashboardConnectionString = configuration["CrawlerStorageConnectionString"],
                StorageConnectionString = configuration["CrawlerStorageConnectionString"],
                NameResolver = new QueueNameResolver(configuration)
            };
            jobconfig.Queues.MaxDequeueCount = configuration.GetValue("CrawlerJobsMaxDequeueCount", 5);
            jobconfig.Queues.MaxPollingInterval =
                TimeSpan.FromSeconds(configuration.GetValue("CrawlerJobsMaxPollingInterval", 30));
            using (var host = new JobHost(jobconfig))
            {
                host.RunAndBlock();
            }
        }

        private class QueueNameResolver : INameResolver
        {
            private readonly IConfiguration config;

            public QueueNameResolver(IConfiguration configuration)
            {
                config = configuration;
            }

            public string Resolve(string name)
            {
                return config[name];
            }
        }

        private class ConsoleLogger : ILogger
        {
            public void Log(LogRecord log)
            {
                Console.WriteLine(log);
            }
        }
    }
}