// <copyright file="MemoryParserJobsQueue.cs" company="DECTech.Tokyo">
// Copyright (c) DECTech.Tokyo. All rights reserved.
// </copyright>

namespace CrawlerLib.Queue
{
    using System;
    using System.Collections.Concurrent;
    using System.Threading;
    using System.Threading.Tasks;
    using Data;

    /// <summary>
    /// In-Memory Parser Jobs queue
    /// </summary>
    public class MemoryParserJobsQueue : IParserJobsQueue
    {
        private readonly ICrawlerStorage crawlerStorage;
        private readonly ConcurrentQueue<IParserJob> jobs = new ConcurrentQueue<IParserJob>();
        private readonly ConcurrentDictionary<string, Session> sessions = new ConcurrentDictionary<string, Session>();
        private readonly SemaphoreSlim tasksSemaphore = new SemaphoreSlim(0, int.MaxValue);

        /// <summary>
        /// Initializes a new instance of the <see cref="MemoryParserJobsQueue" /> class.
        /// </summary>
        /// <param name="crawlerStorage">Crawler Storage service.</param>
        public MemoryParserJobsQueue(ICrawlerStorage crawlerStorage)
        {
            this.crawlerStorage = crawlerStorage;
        }

        /// <inheritdoc />
        public async Task<ICommitableParserJob> DequeueAsync(CancellationToken cancellation)
        {
            await tasksSemaphore.WaitAsync(cancellation);
            if (jobs.TryDequeue(out var job))
            {
                var sess = sessions.GetOrAdd(job.SessionId, id =>
                {
                    var s = new Session();
                    s.Increment();
                    return s;
                });

                return new DummyCommitableParserJob(job, sess, this);
            }

            throw new InvalidOperationException("Queus is empty");
        }

        /// <inheritdoc />
        public async Task EnqueueAsync(IParserJob job, CancellationToken cancellation)
        {
            var sess = sessions.GetOrAdd(job.SessionId, new Session());

            var inserted = await crawlerStorage.EnqueSessionUri(job.SessionId, job.Uri.ToString());
            if (!inserted)
            {
                return;
            }

            jobs.Enqueue(job);

            sess.Increment();

            tasksSemaphore.Release();
        }

        /// <inheritdoc />
        public async Task WaitForSession(string sessionid, CancellationToken cancellation)
        {
            if (sessions.TryGetValue(sessionid, out var sess))
            {
                await sess.WaitAsync(cancellation);
            }
            else
            {
                throw new InvalidOperationException("Session is not found");
            }
        }

        private class Session
        {
            private readonly SemaphoreSlim finished = new SemaphoreSlim(0, 1);
            private int jobs;

            public async Task Decrement(Func<Task> action)
            {
                if (Interlocked.Decrement(ref jobs) == 0)
                {
                    await action();
                    finished.Release();
                }
            }

            public void Increment()
            {
                Interlocked.Increment(ref jobs);
            }

            public async Task WaitAsync(CancellationToken cancellation)
            {
                try
                {
                    await finished.WaitAsync(cancellation);
                }
                finally
                {
                    finished.Dispose();
                }
            }
        }

        private class DummyCommitableParserJob : ICommitableParserJob
        {
            private readonly IParserJob job;
            private readonly MemoryParserJobsQueue queue;
            private readonly Session session;

            public DummyCommitableParserJob(
                IParserJob job,
                Session session,
                MemoryParserJobsQueue queue)
            {
                this.job = job;
                this.session = session;
                this.queue = queue;
            }

            public int Depth => job.Depth;

            public Uri Host => job.Host;

            public int HostDepth => job.HostDepth;

            public string OwnerId => job.OwnerId;

            public ParserParameters ParserParameters => job.ParserParameters;

            public Uri Referrer => job.Referrer;

            public string SessionId => job.SessionId;

            public Uri Uri => job.Uri;

            public async Task Commit(CancellationToken canncellation, int status, string toString)
            {
                await queue.crawlerStorage.UpdateSessionUri(job.SessionId, job.Uri.ToString(), status);

                await session.Decrement(() =>
                {
                    queue.sessions.TryRemove(SessionId, out _);
                    return Task.CompletedTask;
                });
            }
        }
    }
}