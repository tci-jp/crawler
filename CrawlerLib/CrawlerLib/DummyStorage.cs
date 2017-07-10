namespace CrawlerLib
{
    using System;
    using System.Collections.Concurrent;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;

    public class DummyStorage : ICrawlerStorage
    {
        public ConcurrentBag<string> DumpedPages { get; } = new ConcurrentBag<string>();

        public int DumpedPagesNumber => DumpedPages.Count;

        public Task DumpPage(string uri, Stream content)
        {
            DumpedPages.Add(uri);
            return Task.CompletedTask;
        }
    }
}