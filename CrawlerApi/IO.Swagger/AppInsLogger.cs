// <copyright file="AppInsLogger.cs" company="DECTech.Tokyo">
// Copyright (c) DECTech.Tokyo. All rights reserved.
// </copyright>

namespace CrawlerApi
{
    using System;
    using CrawlerLib.Logger;
    using Microsoft.ApplicationInsights;

    /// <summary>
    /// Implementation for App Insight
    /// </summary>
    public class AppInsLogger : ILogger
    {
        private readonly TelemetryClient client;

        /// <summary>
        /// Initializes a new instance of the <see cref="AppInsLogger"/> class.
        /// </summary>
        /// <param name="client">App Insight Client.</param>
        public AppInsLogger(TelemetryClient client)
        {
            this.client = client;
        }

        /// <inheritdoc />
        public void Log(LogRecord log)
        {
            switch (log.Severity)
            {
                case Severity.Error:
                    if (log.Object is Exception ex)
                    {
                        client.TrackException(ex);
                    }
                    else
                    {
                        goto default;
                    }

                    break;
                default:
                    client.TrackTrace(log.ToString());
                    break;
            }
        }
    }
}