// <copyright file="PhantomJSDriver.cs" company="DECTech.Tokyo">
// Copyright (c) DECTech.Tokyo. All rights reserved.
// </copyright>

namespace CrawlerLib.Grabbers
{
    using System;
    using System.Diagnostics;
    using System.Threading;
    using System.Threading.Tasks;

    /// <inheritdoc />
    /// <summary>
    /// PahntomJS wrapper
    /// </summary>
    public class PhantomJSDriver : IDisposable
    {
        private readonly string exePath;
        private readonly string scriptPath;
        private readonly SemaphoreSlim semaphore = new SemaphoreSlim(0, 1);
        private Process process;

        /// <summary>
        /// Initializes a new instance of the <see cref="PhantomJSDriver" /> class.
        /// </summary>
        /// <param name="exePath">Path to executable.</param>
        /// <param name="scriptPath">Path to script.</param>
        public PhantomJSDriver(string exePath = null, string scriptPath = null)
        {
            this.scriptPath = scriptPath ?? "index.js";
            this.exePath = exePath ?? "phantomjs.exe";
        }

        /// <inheritdoc />
        public void Dispose()
        {
            process?.Kill();
            process?.Dispose();
        }

        /// <summary>
        /// Grabs page content.
        /// </summary>
        /// <param name="uri">Page URI.</param>
        /// <param name="cancellation">Cancellation.</param>
        /// <returns>Page content
        /// A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<string> Grab(Uri uri, CancellationToken cancellation)
        {
            process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    FileName = exePath,
                    Arguments = $"\"{scriptPath}\" {uri}"
                },
                EnableRaisingEvents = true
            };

            process.Exited += Process_Exited;
            process.Start();
            var readTask = process.StandardOutput.ReadToEndAsync();
            await Task.WhenAny(readTask, GetTaskCancellation(cancellation));
            cancellation.ThrowIfCancellationRequested();
            await semaphore.WaitAsync(cancellation);

            return await readTask;
        }

        private static Task GetTaskCancellation(CancellationToken token)
        {
            var comp = new TaskCompletionSource<bool>();
            token.Register(() => { comp.SetResult(true); });

            return comp.Task;
        }

        private void Process_Exited(object sender, EventArgs e)
        {
            semaphore.Release();
        }
    }
}