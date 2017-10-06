// <copyright file="DataLakeMetadataStorage.cs" company="DECTech.Tokyo">
// Copyright (c) DECTech.Tokyo. All rights reserved.
// </copyright>

namespace CrawlerLib.Data
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using DECAds.DataLakePlugin;
    using Newtonsoft.Json;

    /// <summary>
    /// DataLake metadata storage.
    /// </summary>
    public class DataLakeMetadataStorage : IMetadataStorage
    {
        private readonly ILogger logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="DataLakeMetadataStorage"/> class.
        /// </summary>
        /// <param name="logger">DataLake logger</param>
        public DataLakeMetadataStorage(ILogger logger)
        {
            this.logger = logger;
        }

        /// <inheritdoc/>
        public async Task DumpUriMetadataAsync(
            string ownerId,
            string sessionId,
            string uri,
            IEnumerable<KeyValuePair<string, string>> metadata,
            CancellationToken cancellation)
        {
            foreach (var pair in metadata)
            {
                var json = JsonConvert.SerializeObject(new LogRecord
                {
                    OwnerId = ownerId,
                    SessionId = sessionId,
                    Uri = uri,
                    Name = pair.Key,
                    Value = pair.Value
                });
                await logger.SendJsonStringAsync(json);
            }
        }

        private class LogRecord
        {
            public string OwnerId { get; set; }

            public string SessionId { get; set; }

            public string Uri { get; set; }

            public string Name { get; set; }

            public string Value { get; set; }
        }
    }
}