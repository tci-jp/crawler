namespace CrawlerLib
{
    using System.IO;
    using System.Net;
    using System.Threading.Tasks;

    public interface ICrawlerStorage
    {
        Task DumpPage(string uri, Stream content);
        
        Task StorePageError(string uri, HttpStatusCode code);
    }
}