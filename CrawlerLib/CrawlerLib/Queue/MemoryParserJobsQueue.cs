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
        /// Initializes a new instance of the <see cref="MemoryParserJobsQueue"/> class.
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
                if (sessions.TryGetValue(job.SessionId, out var sess))
                {
                    return new DummyCommitableParserJob(job, sess, this);
                }

                throw new InvalidOperationException("Session is not found");
            }

            throw new InvalidOperationException("Queus is empty");
        }

        /// <inheritdoc />
        public async Task EnqueueAsync(IParserJob job, CancellationToken cancellation)
        {
            jobs.Enqueue(job);

            Session sess;
            while (!sessions.TryGetValue(job.SessionId, out sess))
            {
                sess = new Session();
                var added = sessions.TryAdd(job.SessionId, sess);
                if (added)
                {
                    break;
                }
            }

            var inserted = await crawlerStorage.EnqueSessionUri(job.SessionId, job.Uri.ToString());
            if (!inserted)
            {
                return;
            }

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

            public void Increment()
            {
                Interlocked.Increment(ref jobs);
            }

            public async Task Decrement(Func<Task> action)
            {
                if (Interlocked.Decrement(ref jobs) == 0)
                {
                    await action();
                    finished.Release();
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

            public Uri Referrer => job.Referrer;

            public string SessionId => job.SessionId;

            public Uri Uri => job.Uri;

            public async Task Commit(CancellationToken canncellation, int status)
            {
                await queue.crawlerStorage.UpdateSessionUri(job.SessionId, job.Uri.ToString(), status);

                await session.Decrement(async () =>
                {
                    queue.sessions.TryRemove(SessionId, out _);
                    await queue.crawlerStorage.UpdateSessionState(job.OwnerId, job.SessionId, SessionState.Done);
                });
            }
        }
    }
}