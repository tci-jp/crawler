// <copyright file="SimpleHttpGrabber.cs" company="DECTech.Tokyo">
// Copyright (c) DECTech.Tokyo. All rights reserved.
// </copyright>

namespace CrawlerLib.Grabbers
{
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using JetBrains.Annotations;
    using Logger;

    /// <inheritdoc />
    /// <summary>
    /// Simple HTTPClient page grabber.
    /// </summary>
    [UsedImplicitly]
    public class SimpleHttpGrabber : HttpGrabber
    {
        private readonly HttpClient client;

        /// <inheritdoc />
        public SimpleHttpGrabber(Configuration config)
            : base(config)
        {
            client = new HttpClient
            {
                Timeout = config.RequestTimeout
            };

            client.DefaultRequestHeaders.UserAgent.ParseAdd(config.UserAgent);
        }

        /// <inheritdoc/>
        public override async Task<GrabResult> Grab(Uri uri, Uri referer)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, uri);
            if (referer != null)
            {
                request.Headers.Referrer = referer;
            }

            var result = await client.SendAsync(request, Config.CancellationToken);
            if (!result.IsSuccessStatusCode)
            {
                Config.Logger.Error(
                    $"{uri} - HttpError: {result.StatusCode}{(int)result.StatusCode}");
                await Task.Delay(Config.RequestErrorRetryDelay);

                // TODO process error;
                return new GrabResult
                {
                    Status = result.StatusCode
                };
            }

            var page = await result.Content.ReadAsStringAsync();
            return new GrabResult
            {
                Status = HttpStatusCode.OK,
                Content = page
            };
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            client?.Dispose();
        }
    }
}