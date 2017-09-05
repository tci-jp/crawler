// <copyright file="AzureParserJobsQueue.cs" company="DECTech.Tokyo">
// Copyright (c) DECTech.Tokyo. All rights reserved.
// </copyright>

namespace CrawlerLib.Azure
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using global::Azure.Storage;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Queue;
    using Microsoft.WindowsAzure.Storage.Table;
    using Newtonsoft.Json;
    using Queue;

    /// <summary>
    /// Azure Storage Queue implementation for parser jobs.
    /// </summary>
    public class AzureParserJobsQueue : IParserJobsQueue
    {
        private readonly TimeSpan dequeueTimeout;
        private readonly OperationContext operationContext = new OperationContext();
        private readonly QueueRequestOptions options = new QueueRequestOptions();
        private readonly string queueName;
        private readonly IDataStorage storage;
        private CloudQueue queue;

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureParserJobsQueue" /> class.
        /// </summary>
        /// <param name="storage">Azure Storage helper class.</param>
        /// <param name="queueName">Name of Azure Storage Queue.</param>
        /// <param name="dequeueTimeout">Time dequeued item does not return to queue if not commited.</param>
        public AzureParserJobsQueue(IDataStorage storage, string queueName, TimeSpan dequeueTimeout)
        {
            this.storage = storage;
            this.queueName = queueName;
            this.dequeueTimeout = dequeueTimeout;
        }

        /// <inheritdoc />
        public async Task<ICommitableParserJob> DequeueAsync(CancellationToken cancellation)
        {
            var q = await GetQueue();
            CloudQueueMessage message;

            do
            {
                try
                {
                    message = await q.GetMessageAsync(dequeueTimeout, options, operationContext, cancellation);
                }
                catch (StorageException ex) when (ex.InnerException is OperationCanceledException)
                {
                    throw ex.InnerException;
                }

                if (message == null)
                {
                    await Task.Delay(200, cancellation);
                }
            }
            while (message == null);

            var job = JsonConvert.DeserializeObject<ParserJob>(message.AsString);
            return new CommitableParserJob(job, q, storage, message);
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
        public async Task WaitForSession(string sessionid, CancellationToken cancellation)
        {
            var query = new TableQuery<SessionUri>()
                .Where(i => (i.SessionId == sessionid) && (i.State == 0));

            while (await storage.CountAsync(query, cancellation) != 0)
            {
                await Task.Delay(500, cancellation);
            }
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
            private readonly CloudQueueMessage message;
            private readonly CloudQueue queue;
            private readonly IDataStorage storage;

            public CommitableParserJob(
                IParserJob job,
                CloudQueue queue,
                IDataStorage storage,
                CloudQueueMessage message)
            {
                this.job = job;
                this.queue = queue;
                this.storage = storage;
                this.message = message;
            }

            public int Depth => job.Depth;

            public Uri Host => job.Host;

            public int HostDepth => job.HostDepth;

            public string OwnerId => job.OwnerId;

            public Uri Referrer => job.Referrer;

            public string SessionId => job.SessionId;

            public Uri Uri => job.Uri;

            public async Task Commit(CancellationToken canncellation, int status, string errorMessage)
            {
                await storage.ReplaceAsync(new SessionUri(job.SessionId, job.Uri.ToString(), status, errorMessage));
                await queue.DeleteMessageAsync(message);
            }
        }
    }
}