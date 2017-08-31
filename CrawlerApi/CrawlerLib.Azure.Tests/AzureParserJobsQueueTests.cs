// <copyright file="AzureParserJobsQueueTests.cs" company="DECTech.Tokyo">
// Copyright (c) DECTech.Tokyo. All rights reserved.
// </copyright>

namespace CrawlerLib.Azure.Tests
{
    using System;
    using System.Diagnostics;
    using System.Threading;
    using System.Threading.Tasks;
    using FluentAssertions;
    using global::Azure.Storage;
    using Microsoft.Extensions.Configuration;
    using Microsoft.WindowsAzure.Storage.Queue;
    using Queue;
    using Xunit;
    using Xunit.Abstractions;

    public class AzureParserJobsQueueTests : IDisposable
    {
        private const string OwnerId = "testowner";
        private readonly CancellationTokenSource cancellation = new CancellationTokenSource(TimeSpan.FromSeconds(60));
        private readonly CloudQueue cloudqueue;

        private readonly ITestOutputHelper output;
        private readonly AzureParserJobsQueue queue;

        private readonly ParserJob testJob = new ParserJob
        {
            Depth = 1,
            HostDepth = 2,
            Uri = new Uri("http://dectech.tokyo"),
            OwnerId = OwnerId,
            Referrer = new Uri("http://localhost"),
            SessionId = "asfsgfh"
        };

        private readonly AzureCrawlerStorage crawlerStorage;

        public AzureParserJobsQueueTests(ITestOutputHelper output)
        {
            this.output = output;

            var queueName = "testqueue" + Guid.NewGuid();

            var builder = new ConfigurationBuilder()
                .AddJsonFile("config.json", true, true)
                .AddEnvironmentVariables();
            var configuration = builder.Build();
            var storage = new DataStorage(configuration["CrawlerStorageConnectionString"]);
            crawlerStorage = new AzureCrawlerStorage(storage, null);
            queue = new AzureParserJobsQueue(storage, queueName, TimeSpan.FromMinutes(1));
            cloudqueue = storage.QueueClient.GetQueueReference(queueName);
            cloudqueue.CreateIfNotExistsAsync().GetAwaiter().GetResult();

            var sessionId = crawlerStorage.CreateSession(OwnerId, new[] { "http://dectech.tokyo" }).GetAwaiter()
                                          .GetResult();
            testJob.SessionId = sessionId;
        }

        public void Dispose()
        {
            output.WriteLine("Started disposing.");
            cancellation.Dispose();
            cloudqueue.DeleteIfExistsAsync().GetAwaiter().GetResult();
            output.WriteLine("Finished disposing.");
        }

        [Fact]
        public async Task TestCancel()
        {
            IParserJob job = null;
            var started = false;
            var finished = false;
            var error = false;
            var cancelled = false;
            var task = Task.Run(async () =>
            {
                try
                {
                    started = true;
                    job = await queue.DequeueAsync(cancellation.Token);
                }
                catch (OperationCanceledException)
                {
                    cancelled = true;
                }
                catch (Exception ex)
                {
                    error = true;
                    output.WriteLine(ex.ToString());
                }
                finally
                {
                    finished = true;
                }
            });

            await Task.Delay(1000);

            started.Should().Be(true);
            finished.Should().Be(false);
            error.Should().Be(false, "Error");
            cancelled.Should().Be(false);

            cancellation.Cancel();

            await task;

            started.Should().Be(true);
            finished.Should().Be(true);
            error.Should().Be(false, "Error");
            cancelled.Should().Be(true);

            job.Should().BeNull();
        }

        [Fact]
        public async Task TestDequeueBlock()
        {
            IParserJob job = null;
            var started = false;
            var finished = false;
            var error = false;
            var task = Task.Run(async () =>
            {
                try
                {
                    await cloudqueue.FetchAttributesAsync();
                    output.WriteLine("Started dequeue." +
                                     (cloudqueue.ApproximateMessageCount?.ToString() ?? "Count null"));
                    started = true;
                    var watch = Stopwatch.StartNew();
                    job = await queue.DequeueAsync(cancellation.Token);
                    if (job == null)
                    {
                        await cloudqueue.FetchAttributesAsync();
                        output.WriteLine(cloudqueue.ApproximateMessageCount?.ToString() ?? "Count null");
                    }

                    output.WriteLine("Finished dequeue. " + watch.ElapsedMilliseconds);
                }
                catch (Exception ex)
                {
                    error = true;
                    output.WriteLine(ex.ToString());
                }
                finally
                {
                    finished = true;
                }
            });

            await Task.Delay(1000);

            started.Should().Be(true);
            finished.Should().Be(false);
            error.Should().Be(false, "Error");

            await queue.EnqueueAsync(testJob, CancellationToken.None);
            await cloudqueue.FetchAttributesAsync();
            output.WriteLine("Finished enqueue. " + (cloudqueue.ApproximateMessageCount?.ToString() ?? "Count null"));

            await task;

            started.Should().Be(true);
            finished.Should().Be(true);
            error.Should().Be(false, "Error");

            job.ShouldBeEquivalentTo(testJob);
        }

        [Fact]
        public async Task TestEnqueueDequeue()
        {
            await queue.EnqueueAsync(testJob, cancellation.Token);

            var job = await queue.DequeueAsync(cancellation.Token);

            job.ShouldBeEquivalentTo(testJob);
        }

        [Fact]
        public async Task TestWaitForSession()
        {
            var started = false;
            var finished = false;
            var error = false;
            var cancelled = false;

            var stop = Stopwatch.StartNew();
            for (var i = 0; i < 10; i++)
            {
                testJob.Uri = new Uri("http://dectech.tokyo" + i);
                await queue.EnqueueAsync(testJob, cancellation.Token);
            }

            output.WriteLine("Finished enqueue: " + stop.ElapsedMilliseconds);

            var task = Task.Run(async () =>
            {
                var stop2 = Stopwatch.StartNew();
                try
                {
                    started = true;
                    output.WriteLine("Started waiting.");
                    await queue.WaitForSession(testJob.SessionId, cancellation.Token);
                    output.WriteLine("Finished waiting.");
                }
                catch (OperationCanceledException)
                {
                    cancelled = true;
                }
                catch (Exception ex)
                {
                    error = true;
                    output.WriteLine("Failed waiting: {0} {1}", stop2.ElapsedMilliseconds, ex);
                }
                finally
                {
                    finished = true;
                }
            });

            await Task.Delay(1000);

            started.Should().Be(true);
            finished.Should().Be(false);
            error.Should().Be(false, "Error");
            cancelled.Should().Be(false);

            stop = Stopwatch.StartNew();
            ICommitableParserJob job;
            for (var i = 0; i < 9; i++)
            {
                job = await queue.DequeueAsync(cancellation.Token);
                await job.Commit(cancellation.Token, 200);
            }

            started.Should().Be(true);
            finished.Should().Be(false);
            error.Should().Be(false, "Error");
            cancelled.Should().Be(false);

            job = await queue.DequeueAsync(cancellation.Token);
            await job.Commit(cancellation.Token, 200);

            output.WriteLine("Dequeued all: " + stop.ElapsedMilliseconds);

            await task;

            started.Should().Be(true);
            finished.Should().Be(true);
            error.Should().Be(false, "Error");
            cancelled.Should().Be(false);
        }
    }
}