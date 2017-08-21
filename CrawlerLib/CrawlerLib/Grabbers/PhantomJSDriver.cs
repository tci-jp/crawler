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
    public class PhantomJsDriver : IDisposable
    {
        private readonly string exePath;
        private readonly string scriptPath;
        private Process process;

        /// <summary>
        /// Initializes a new instance of the <see cref="PhantomJsDriver" /> class.
        /// </summary>
        /// <param name="exePath">Path to executable.</param>
        /// <param name="scriptPath">Path to script.</param>
        public PhantomJsDriver(string exePath = null, string scriptPath = null)
        {
            this.scriptPath = scriptPath ?? "Grabbers\\script.js";
            this.exePath = exePath ?? "phantomjs.exe";
        }

        /// <inheritdoc />
        public void Dispose()
        {
            if (process != null)
            {
                if (!process.HasExited)
                {
                    process?.Kill();
                }

                process.Dispose();
            }
        }

        /// <summary>
        /// Grabs page content.
        /// </summary>
        /// <param name="uri">Page URI.</param>
        /// <param name="cancellation">Cancellation.</param>
        /// <returns>
        /// Page content
        /// A <see cref="Task" /> representing the asynchronous operation.
        /// </returns>
        public async Task<string> Grab(Uri uri, CancellationToken cancellation)
        {
            process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    FileName = exePath,
                    Arguments = $"--load-images=false \"{scriptPath}\" {uri}"
                },
                EnableRaisingEvents = true
            };

            cancellation.Register(() => { process.Kill(); });

            process.Start();
            var page = await process.StandardOutput.ReadToEndAsync();
            var errors = await process.StandardError.ReadToEndAsync();
            return page;
        }
    }
}