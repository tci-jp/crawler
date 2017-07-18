namespace CrawlerLib
{
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Logger;

    public class SimpleHttpGrabber : HttpGrabber
    {
        private readonly Configuration config;
        private readonly HttpClient client;

        public SimpleHttpGrabber(Configuration config)
            : base(config)
        {
            this.config = config;
            client = new HttpClient
                     {
                         Timeout = config.RequestTimeout
                     };

            client.DefaultRequestHeaders.UserAgent.ParseAdd(config.UserAgent);
        }

        public override async Task<GrabResult> Grab(Uri uri, Uri referer)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, uri);
            if (referer != null)
            {
                request.Headers.Referrer = referer;
            }

            var result = await client.SendAsync(request, config.CancellationToken);
            if (!result.IsSuccessStatusCode)
            {
                config.Logger.Error(
                    $"{uri} - HttpError: {result.StatusCode}{(int)result.StatusCode}");
                await Task.Delay(config.RequestErrorRetryDelay);

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
    }
}