using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Azure.Storage;
using CrawlerLib.Azure;

namespace Test
{
    [TestClass]
    public class SearchTest
    {
        
        public async Task Search(string text)
        {
            const string connectionString =
                "DefaultEndpointsProtocol=https;AccountName=indexing;AccountKey=566Nz2py6Pir6p+JRcQ5QveAJf6Yv/MwZgHSVpi4vLL4QJyXiC3AujSwU47117b9KRFFlMd/NLhJpNuVjxCCYg==;";
            var storage = new DataStorage(connectionString);
            var blobSearcher = new SimpleBlobSearcher2(storage, "indexing-container");

            await blobSearcher.SearchByText(text);
        }

        [TestMethod]
        public async Task Main()
        {
            await Search("tech");
        }
    }
}
