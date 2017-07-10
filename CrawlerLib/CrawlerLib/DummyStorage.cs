namespace CrawlerLib
{
    using System;
    using System.Collections.Concurrent;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;

    public class UriDump
    {
        public string Uri { get; set; }

        public string Content { get; set; }
    }

    public class DummyStorage : ICrawlerStorage
    {
        public ConcurrentBag<UriDump> DumpedPages { get; } = new ConcurrentBag<UriDump>();

        public int DumpedPagesNumber => DumpedPages.Count;

        public async Task DumpPage(string uri, Stream content)
        {
            var reader = new StreamReader(content);
            DumpedPages.Add(new UriDump { Uri = uri, Content = await reader.ReadToEndAsync() });
        }
    }
}