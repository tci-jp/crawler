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

    public class QueuedTaskRunner
    {
        private readonly TimeSpan delay;

        private readonly AsyncCollection<Task> queue = new AsyncCollection<Task>(new ConcurrentQueue<Task>());
        private readonly CancellationToken token;

        public QueuedTaskRunner(TimeSpan delay, CancellationToken token)
        {
            this.delay = delay;
            this.token = token;

            Task.Run(RunQueue);
        }

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