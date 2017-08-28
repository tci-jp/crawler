// <copyright file="QueuedTaskRunner.cs" company="DECTech.Tokyo">
// Copyright (c) DECTech.Tokyo. All rights reserved.
// </copyright>

namespace CrawlerLib
{
    using System;
    using System.Collections.Concurrent;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Runs tasks in queue with delay between.
    /// </summary>
    public class QueuedTaskRunner
    {
        private readonly TimeSpan delay;

        private readonly ConcurrentQueue<Task> queue = new ConcurrentQueue<Task>();
        private readonly SemaphoreSlim tasksSemaphore = new SemaphoreSlim(0, int.MaxValue);
        private readonly CancellationToken token;

        /// <summary>
        /// Initializes a new instance of the <see cref="QueuedTaskRunner" /> class.
        /// Creates ans run queue.
        /// </summary>
        /// <param name="delay">Delay between tasks.</param>
        /// <param name="token">Cancellation Token.</param>
        public QueuedTaskRunner(TimeSpan delay, CancellationToken token)
        {
            this.delay = delay;
            this.token = token;

            Task.Run(RunQueue);
        }

        /// <summary>
        /// Adds new not-started task into the queue.
        /// </summary>
        /// <param name="task">
        /// Task to queue.
        /// Should not be started, Task.TaskStatus should be WaitingToStart.
        /// </param>
        public void Enqueue(Task task)
        {
            queue.Enqueue(task);
            tasksSemaphore.Release();
        }

        private async Task RunQueue()
        {
            while (!token.IsCancellationRequested)
            {
                await tasksSemaphore.WaitAsync(token);
                if (!queue.TryDequeue(out var task))
                {
                    throw new InvalidOperationException("Queue should never be empty after semapthore waiting.");
                }

                task.Start();
                await Task.Delay(delay, token);
            }
        }
    }
}