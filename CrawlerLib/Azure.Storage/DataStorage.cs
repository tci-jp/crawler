// <copyright file="DataStorage.cs" company="DECTech.Tokyo">
// Copyright (c) DECTech.Tokyo. All rights reserved.
// </copyright>

namespace Azure.Storage
{
    using System;
    using System.Collections.Async;
    using System.Collections.Concurrent;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Net;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using JetBrains.Annotations;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Blob;
    using Microsoft.WindowsAzure.Storage.Table;

    /// <summary>
    /// Helper class for Azure Storage Blobs and Tables.
    /// </summary>
    [UsedImplicitly]
    public class DataStorage : IDataStorage
    {
        private static readonly ConcurrentDictionary<Type, TableAttribute> TypeToAttribute =
            new ConcurrentDictionary<Type, TableAttribute>();

        /// <summary>
        /// Initializes a new instance of the <see cref="DataStorage" /> class.
        /// </summary>
        /// <param name="connectionString">Azure Storage connection string</param>
        public DataStorage(string connectionString)
        {
            StorageAccount = CloudStorageAccount.Parse(connectionString);
            TableClient = StorageAccount.CreateCloudTableClient();
            BlobClient = StorageAccount.CreateCloudBlobClient();
        }

        /// <inheritdoc />
        [UsedImplicitly]
        public CloudBlobClient BlobClient { get; }

        private CloudStorageAccount StorageAccount { get; }

        private CloudTableClient TableClient { get; }

        /// <inheritdoc />
        [UsedImplicitly]
        public async Task<bool> DeleteAsync<TEntity>(TEntity entity)
            where TEntity : TableEntity
        {
            var retrieveOperation = TableOperation.Delete(entity);
            var retrievedResult = await GetTable<TEntity>()
                                      .ExecuteAsync(retrieveOperation)
                                      .ConfigureAwait(false);
            return retrievedResult.HttpStatusCode == 200;
        }

        /// <inheritdoc />
        public IAsyncEnumerable<TEntity> ExecuteQuery<TEntity>(
            TableQuery<TEntity> query,
            CancellationToken token = default(CancellationToken))
            where TEntity : TableEntity, new()
        {
            var table = GetTable<TEntity>();
            var requestOption = new TableRequestOptions();
            var context = new OperationContext();

            return new AsyncEnumerable<TEntity>(
                async yield =>
                {
                    try
                    {
                        TableQuerySegment<TEntity> segment = null;
                        do
                        {
                            segment = await table.ExecuteQuerySegmentedAsync(
                                          query,
                                          segment?.ContinuationToken,
                                          requestOption,
                                          context,
                                          token).ConfigureAwait(false);
                            if (segment == null)
                            {
                                break;
                            }

                            foreach (var item in segment)
                            {
                                token.ThrowIfCancellationRequested();
                                await yield.ReturnAsync(item);
                            }
                        }
                        while (segment.ContinuationToken != null);
                    }
                    catch (StorageException ex)
                        when (ex.InnerException is WebException webex
                              && webex.Response is HttpWebResponse resp
                              && (resp.StatusCode == HttpStatusCode.NotFound))
                    {
                        await table.CreateAsync();
                    }
                });
        }

        /// <inheritdoc />
        [UsedImplicitly]
        public async Task<CloudBlobContainer> GetBlobContainerAsync(string name)
        {
            var container = BlobClient.GetContainerReference(name);
            await container.CreateIfNotExistsAsync();
            return container;
        }

        /// <inheritdoc />
        [UsedImplicitly]
        public CloudTable GetTable<T>()
        {
            var result = TableClient.GetTableReference(GetEntityTable<T>());
            return result;
        }

        /// <inheritdoc />
        [UsedImplicitly]
        public async Task InsertAsync<TEntity>(TEntity entity)
            where TEntity : TableEntity
        {
            PrepareEntity(entity);
            var cloudTable = await GetOrCreateTableAsync<TEntity>();
            ProcessResult(await cloudTable.ExecuteAsync(TableOperation.Insert(entity)));
        }

        /// <inheritdoc />
        [UsedImplicitly]
        public async Task InsertOrReplaceAsync<TEntity>(TEntity entity)
            where TEntity : TableEntity
        {
            PrepareEntity(entity);
            var cloudTable = await GetOrCreateTableAsync<TEntity>();
            var insertOrReplace = TableOperation.InsertOrReplace(entity);
            var tableResult = await cloudTable.ExecuteAsync(insertOrReplace);
            ProcessResult(tableResult);
        }

        /// <inheritdoc />
        [UsedImplicitly]
        public IAsyncEnumerable<TEntity> QueryAsync<TEntity>(
            TEntity entity,
            CancellationToken token = default(CancellationToken))
            where TEntity : TableEntity, new()
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            return ExecuteQuery(Query(entity), token);
        }

        /// <inheritdoc />
        [UsedImplicitly]
        public IAsyncEnumerable<TEntity> QueryAsync<TEntity>(
            Expression<Func<TEntity, bool>> func,
            CancellationToken token = default(CancellationToken))
            where TEntity : TableEntity, new()
        {
            var visitor = new PropertyReplacer<TEntity>();
            var newfunc = visitor.VisitAndConvert(func);

            var query = Query<TEntity>().Where(newfunc);
            return ExecuteQuery(query, token);
        }

        /// <inheritdoc />
        [UsedImplicitly]
        public IAsyncEnumerable<TEntity> QueryAsync<TEntity>(CancellationToken token = default(CancellationToken))
            where TEntity : TableEntity, new()
        {
            return ExecuteQuery(Query<TEntity>(), token);
        }

        /// <inheritdoc />
        [UsedImplicitly]
        public async Task<TableQuerySegment<TEntity>> QuerySegmentedAsync<TEntity>(
            Expression<Func<TEntity, bool>> func,
            int segmentSize = 10,
            TableContinuationToken token = null,
            CancellationToken cancellation = default(CancellationToken))
            where TEntity : TableEntity, new()
        {
            var visitor = new PropertyReplacer<TEntity>();
            var newfunc = visitor.VisitAndConvert(func);

            var query = Query<TEntity>().Where(newfunc);
            query.TakeCount = segmentSize;

            var table = GetTable<TEntity>();
            var requestOption = new TableRequestOptions();
            var context = new OperationContext();

            return await table.ExecuteQuerySegmentedAsync(
                                  query,
                                  token,
                                  requestOption,
                                  context,
                                  cancellation)
                              .ConfigureAwait(false);
        }

        /// <inheritdoc />
        [UsedImplicitly]
        public async Task ReplaceAsync<TEntity>(TEntity entity)
            where TEntity : TableEntity
        {
            PrepareEntity(entity);
            ProcessResult(await (await GetOrCreateTableAsync<TEntity>()).ExecuteAsync(TableOperation.Replace(entity)));
        }

        /// <inheritdoc />
        [UsedImplicitly]
        public async Task<TEntity> RetreiveAsync<TEntity>(string partitionKey, string rowKey)
            where TEntity : TableEntity
        {
            var retrieveOperation = TableOperation.Retrieve<TEntity>(partitionKey, rowKey);
            var retrievedResult = await GetTable<TEntity>().ExecuteAsync(retrieveOperation);
            return (TEntity)retrievedResult?.Result;
        }

        /// <inheritdoc />
        [UsedImplicitly]
        public Task<TEntity> RetreiveAsync<TEntity>(TEntity entity)
            where TEntity : TableEntity
        {
            var attr = GetEntityAttribute<TEntity>();
            var partition = entity.PartitionKey ?? attr.PartitionKey;
            var key = entity.RowKey ?? attr.RowKey;
            return RetreiveAsync<TEntity>(partition, key);
        }

        /// <inheritdoc />
        [UsedImplicitly]
        public Task<TEntity> RetreiveAsync<TEntity>(string rowKey)
            where TEntity : TableEntity
        {
            return RetreiveAsync<TEntity>(GetEntityPartiton<TEntity>(), rowKey);
        }

        /// <inheritdoc />
        [UsedImplicitly]
        public Task<TEntity> RetreiveAsync<TEntity>()
            where TEntity : TableEntity
        {
            var attr = GetEntityAttribute<TEntity>();
            return RetreiveAsync<TEntity>(attr.PartitionKey, attr.RowKey);
        }

        /// <inheritdoc />
        [UsedImplicitly]
        public async Task<TEntity> RetreiveOrCreateAsync<TEntity>(TEntity entity)
            where TEntity : TableEntity
        {
            var attr = GetEntityAttribute<TEntity>();
            var partition = entity.PartitionKey ?? attr.PartitionKey;
            var key = entity.RowKey ?? attr.RowKey;
            return await RetreiveAsync<TEntity>(partition, key) ?? entity;
        }

        private static TableAttribute GetEntityAttribute<T>()
        {
            return TypeToAttribute.GetOrAdd(typeof(T), t =>
            {
                var attr = t.GetTypeInfo()
                            .GetCustomAttributes(typeof(TableAttribute), true)
                            .SingleOrDefault() as TableAttribute;
                if (attr == null)
                {
                    throw new ArgumentException(
                        $"Type {typeof(T)} does not have Table attribute");
                }

                return attr;
            });
        }

        private static string GetEntityPartiton<T>()
        {
            return GetEntityAttribute<T>().PartitionKey ??
                   throw new ArgumentException($"Type {typeof(T)} does not have default Partition");
        }

        private static string GetEntityTable<T>()
        {
            return GetEntityAttribute<T>().Table;
        }

        private static void PrepareEntity<TEntity>(TEntity entity)
            where TEntity : TableEntity
        {
            var attr = GetEntityAttribute<TEntity>();
            if (entity.PartitionKey == null)
            {
                entity.PartitionKey = attr.PartitionKey;
            }

            if (entity.RowKey == null)
            {
                entity.RowKey = attr.RowKey;
            }
        }

        private static void ProcessResult(TableResult result)
        {
            switch (result.HttpStatusCode)
            {
                case (int)HttpStatusCode.OK:
                case (int)HttpStatusCode.Accepted:
                case (int)HttpStatusCode.Created:
                case (int)HttpStatusCode.NoContent:
                    return;

                case (int)HttpStatusCode.Conflict: throw new ArgumentException("Partition and key already exist");

                default: throw new Exception($"Request failed: {result.HttpStatusCode} - {result.Result}");
            }
        }

        private static TableQuery<TEntity> Query<TEntity>(TEntity entity)
            where TEntity : TableEntity, new()
        {
            var attr = GetEntityAttribute<TEntity>();
            var query = new TableQuery<TEntity>();
            if ((attr.PartitionKey != null) && (attr.RowKey == null) && (entity.RowKey != null))
            {
                return query.Where(i => (i.PartitionKey == attr.PartitionKey) && (i.RowKey == entity.RowKey));
            }

            if ((attr.PartitionKey == null) && (attr.RowKey != null) && (entity.PartitionKey != null))
            {
                return query.Where(i => (i.RowKey == attr.RowKey) && (i.PartitionKey == entity.PartitionKey));
            }

            return Query<TEntity>();
        }

        private static TableQuery<TEntity> Query<TEntity>()
            where TEntity : TableEntity, new()
        {
            var attr = GetEntityAttribute<TEntity>();

            var query = new TableQuery<TEntity>();
            if ((attr.PartitionKey != null) && (attr.RowKey != null))
            {
                query = query.Where(i => (i.PartitionKey == attr.PartitionKey) && (i.RowKey == attr.RowKey));
            }
            else if ((attr.PartitionKey != null) && (attr.RowKey == null))
            {
                query = query.Where(i => i.PartitionKey == attr.PartitionKey);
            }
            else if ((attr.PartitionKey == null) && (attr.RowKey != null))
            {
                query = query.Where(i => i.RowKey == attr.RowKey);
            }

            return query;
        }

        private async Task<CloudTable> GetOrCreateTableAsync<T>()
        {
            var result = TableClient.GetTableReference(GetEntityTable<T>());
            await result.CreateIfNotExistsAsync();
            return result;
        }
    }
}