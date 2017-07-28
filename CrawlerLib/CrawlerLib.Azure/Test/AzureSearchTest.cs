using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure.Storage;

namespace CrawlerLib.Azure.Test
{
    public class AzureSearchTest
    {
        public async Task Test()
        {
            const string connectionString =
                "DefaultEndpointsProtocol=https;AccountName=indexing;AccountKey=566Nz2py6Pir6p+JRcQ5QveAJf6Yv/MwZgHSVpi4vLL4QJyXiC3AujSwU47117b9KRFFlMd/NLhJpNuVjxCCYg==;";
            var storage = new DataStorage(connectionString);
            SimpleBlobSearcher blobSearcher = new SimpleBlobSearcher(storage, connectionString);

            await blobSearcher.SearchByText("test");

        }
    }
}
