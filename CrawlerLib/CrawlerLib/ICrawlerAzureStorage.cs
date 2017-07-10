namespace CrawlerLib
{
    using System.IO;
    using System.Threading.Tasks;

    public interface ICrawlerStorage
    {
        Task DumpPage(string uri, Stream content);
    }
}