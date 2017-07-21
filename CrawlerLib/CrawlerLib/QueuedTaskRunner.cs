// <copyright file="QueuedTaskRunner.cs" company="DECTech.Tokyo">
// Copyright (c) DECTech.Tokyo. All rights reserved.
// </copyright>

namespace CrawlerLib
{
    using System;
    using System.Collections.Concurrent;
    using System.Threading;
    using System.Threading.Tasks;
    using Nito.AsyncEx;

    /// <summary>
    /// Runs tasks in queue with delay between.
    /// </summary>
    public class QueuedTaskRunner
    {
        private readonly TimeSpan delay;

        private readonly AsyncCollection<Task> queue = new AsyncCollection<Task>(new ConcurrentQueue<Task>());
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
            queue.Add(task, token);
        }

        private async Task RunQueue()
        {
            while (!token.IsCancellationRequested)
            {
                var task = await queue.TakeAsync(token);
                task.Start();
                await Task.Delay(delay, token);
            }
        }
    }
}