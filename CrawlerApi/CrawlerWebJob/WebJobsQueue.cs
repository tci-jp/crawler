// <copyright file="WebJobsQueue.cs" company="DECTech.Tokyo">
// Copyright (c) DECTech.Tokyo. All rights reserved.
// </copyright>

namespace CrawlerWebJob
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Azure.Storage;
    using CrawlerLib.Azure;
    using CrawlerLib.Queue;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Queue;
    using Newtonsoft.Json;

    /// <summary>
    /// Azure Storage Queue implementation for parser jobs.
    /// </summary>
    public class WebJobsQueue : IParserJobsQueue
    {
        private readonly TimeSpan dequeueTimeout;
        private readonly OperationContext operationContext = new OperationContext();
        private readonly QueueRequestOptions options = new QueueRequestOptions();
        private readonly string queueName;
        private readonly IDataStorage storage;
        private CloudQueue queue;

        /// <summary>
        /// Initializes a new instance of the <see cref="WebJobsQueue" /> class.
        /// </summary>
        /// <param name="storage">Azure Storage helper class.</param>
        /// <param name="queueName">Name of Azure Storage Queue.</param>
        /// <param name="dequeueTimeout">Time dequeued item does not return to queue if not commited.</param>
        public WebJobsQueue(IDataStorage storage, string queueName, TimeSpan dequeueTimeout)
        {
            this.storage = storage;
            this.queueName = queueName;
            this.dequeueTimeout = dequeueTimeout;
        }

        public ICommitableParserJob ConvertMessage(string message)
        {
            var job = JsonConvert.DeserializeObject<ParserJob>(message);
            return new CommitableParserJob(job, storage);
        }

        /// <inheritdoc />
        public Task<ICommitableParserJob> DequeueAsync(CancellationToken cancellation)
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc />
        public async Task EnqueueAsync(IParserJob job, CancellationToken cancellation)
        {
            var q = await GetQueue();
            var content = JsonConvert.SerializeObject(job);
            var message = new CloudQueueMessage(content);

            var inserted = await storage.InsertAsync(new SessionUri(job.SessionId, job.Uri.ToString(), 0));
            if (!inserted)
            {
                return;
            }

            await q.AddMessageAsync(message);
        }

        /// <inheritdoc />
        public Task WaitForSession(string sessionid, CancellationToken cancellation)
        {
            throw new NotSupportedException();
        }

        private async Task<CloudQueue> GetQueue()
        {
            if (queue != null)
            {
                return queue;
            }

            queue = storage.QueueClient.GetQueueReference(queueName);
            await queue.CreateIfNotExistsAsync();

            return queue;
        }

        private class CommitableParserJob : ICommitableParserJob
        {
            private readonly IParserJob job;
            private readonly IDataStorage storage;

            public CommitableParserJob(
                IParserJob job,
                IDataStorage storage)
            {
                this.job = job;
                this.storage = storage;
            }

            public int Depth => job.Depth;

            public Uri Host => job.Host;

            public int HostDepth => job.HostDepth;

            public string OwnerId => job.OwnerId;

            public QueueParserParameters ParserParameters => job.ParserParameters;

            public Uri Referrer => job.Referrer;

            public string SessionId => job.SessionId;

            public Uri Uri => job.Uri;

            public async Task Commit(CancellationToken canncellation, int status, string errorMessage)
            {
                await storage.ReplaceAsync(new SessionUri(job.SessionId, job.Uri.ToString(), status, errorMessage));
            }
        }
    }
}