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
    using System.Text;
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
    public class DataStorage
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

        /// <summary>
        /// Gets Azure Cloud Blob Client
        /// </summary>
        [UsedImplicitly]
        public CloudBlobClient BlobClient { get; }

        private CloudStorageAccount StorageAccount { get; }

        private CloudTableClient TableClient { get; }

        /// <summary>
        /// Decodes string from Base64.
        /// </summary>
        /// <param name="code">Base64 string.</param>
        /// <returns>Decoded string.</returns>
        public static string DecodeString(string code)
        {
            return Encoding.UTF8.GetString(Convert.FromBase64String(code));
        }

        /// <summary>
        /// Encodes string into Base64.
        /// </summary>
        /// <param name="str">String to encode.</param>
        /// <returns>Encoded string.</returns>
        public static string EncodeString(string str)
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(str));
        }

        /// <summary>
        /// Deletes Table entity with specific PartitionKey and RowKey.
        /// </summary>
        /// <param name="entity">Entity to delete.</param>
        /// <typeparam name="TEntity">Type of Table entity</typeparam>
        /// <returns>A <see cref="Task" /> representing the asynchronous operation.</returns>
        [UsedImplicitly]
        public async Task<bool> DeleteAsync<TEntity>(TEntity entity)
            where TEntity : TableEntity
        {
            var retrieveOperation = TableOperation.Delete(entity);
            var retrievedResult = await GetTable<TEntity>().ExecuteAsync(retrieveOperation);
            return retrievedResult.HttpStatusCode == 200;
        }

        /// <summary>
        /// Executes asynchronuous query
        /// </summary>
        /// <typeparam name="TEntity">Table entity.</typeparam>
        /// <param name="query">Query to execute.</param>
        /// <param name="token">Cancellation.</param>
        /// <returns>Async enumerable with result.</returns>
        public IAsyncEnumerable<TEntity> ExecuteQuery<TEntity>(TableQuery<TEntity> query, CancellationToken token = default(CancellationToken))
            where TEntity : TableEntity, new()
        {
            var table = GetTable<TEntity>();
            var requestOption = new TableRequestOptions();
            var context = new OperationContext();

            return new AsyncEnumerable<TEntity>(
                async yield =>
                {
                    TableQuerySegment<TEntity> segment = null;
                    while ((segment = await table.ExecuteQuerySegmentedAsync(
                                          query,
                                          segment?.ContinuationToken,
                                          requestOption,
                                          context,
                                          token).ConfigureAwait(false)) != null)
                    {
                        foreach (var item in segment)
                        {
                            token.ThrowIfCancellationRequested();
                            yield?.ReturnAsync(item);
                        }
                    }
                });
        }

        /// <summary>
        /// Gets Blob Container for this Azure Storage.
        /// </summary>
        /// <param name="name">Blob Container name.</param>
        /// <returns>Blob Container object. A <see cref="Task" /> representing the asynchronous operation.</returns>
        [UsedImplicitly]
        public async Task<CloudBlobContainer> GetBlobContainerAsync(string name)
        {
            var container = BlobClient.GetContainerReference(name);
            await container.CreateIfNotExistsAsync();
            return container;
        }

        /// <summary>
        /// Gets Table object by type.
        /// </summary>
        /// <typeparam name="T">Type of Table.</typeparam>
        /// <returns>Table object.</returns>
        [UsedImplicitly]
        public CloudTable GetTable<T>()
        {
            var result = TableClient.GetTableReference(GetEntityTable<T>());
            return result;
        }

        /// <summary>
        /// Inserts new entity into table. No existing entity with the same Row and Partition keys is allowed.
        /// </summary>
        /// <param name="entity">Entity to insert.</param>
        /// <typeparam name="TEntity">Entity type.</typeparam>
        /// <returns>A <see cref="Task" /> representing the asynchronous operation.</returns>
        [UsedImplicitly]
        public async Task InsertAsync<TEntity>(TEntity entity)
            where TEntity : TableEntity
        {
            PrepareEntity(entity);
            var cloudTable = await GetOrCreateTableAsync<TEntity>();
            ProcessResult(await cloudTable.ExecuteAsync(TableOperation.Insert(entity)));
        }

        /// <summary>
        /// Inserts entity into table. If entity with the same Row and Partition keys exists it get replaced.
        /// </summary>
        /// <param name="entity">Entity to insert.</param>
        /// <typeparam name="TEntity">Entity type.</typeparam>
        /// <returns>A <see cref="Task" /> representing the asynchronous operation.</returns>
        [UsedImplicitly]
        public async Task InsertOrReplaceAsync<TEntity>(TEntity entity)
            where TEntity : TableEntity
        {
            PrepareEntity(entity);
            var cloudTable = await GetOrCreateTableAsync<TEntity>();
            ProcessResult(await cloudTable.ExecuteAsync(TableOperation.InsertOrReplace(entity)));
        }

        /// <summary>
        /// Starts query for specific Table type.
        /// </summary>
        /// <typeparam name="TEntity">Type of Table.</typeparam>
        /// <param name="entity">Entity with not null PartitionKey or RowKey for query</param>
        /// <param name="token">Cancellation</param>
        /// <returns>Queryable for further quering.</returns>
        [UsedImplicitly]
        public IAsyncEnumerable<TEntity> QueryAsync<TEntity>(
            [NotNull] TEntity entity,
            CancellationToken token = default(CancellationToken))
            where TEntity : TableEntity, new()
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            return ExecuteQuery(Query(entity), token);
        }

        /// <summary>
        /// Query Azure Table by expression
        /// </summary>
        /// <param name="func">Query expression.</param>
        /// <typeparam name="TEntity">Type of Entity.</typeparam>
        /// <param name="token">Cancellation</param>
        /// <returns>Queryable with result.</returns>
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

        /// <summary>
        /// Starts query for specific Table type.
        /// </summary>
        /// <typeparam name="TEntity">Type of Table.</typeparam>
        /// <param name="token">Cancellation</param>
        /// <returns>Queryable for further quering.</returns>
        [UsedImplicitly]
        public IAsyncEnumerable<TEntity> QueryAsync<TEntity>(CancellationToken token = default(CancellationToken))
            where TEntity : TableEntity, new()
        {
            return ExecuteQuery(Query<TEntity>(), token);
        }

        /// <summary>
        /// Replace only if entity already exists.
        /// </summary>
        /// <param name="entity">Entity to replace.</param>
        /// <typeparam name="TEntity">Type of Entity.</typeparam>
        /// <returns>A <see cref="Task" /> representing the asynchronous operation.</returns>
        [UsedImplicitly]
        public async Task ReplaceAsync<TEntity>(TEntity entity)
            where TEntity : TableEntity
        {
            PrepareEntity(entity);
            ProcessResult(await (await GetOrCreateTableAsync<TEntity>()).ExecuteAsync(TableOperation.Replace(entity)));
        }

        /// <summary>
        /// Gets entity by partition and row keys.
        /// </summary>
        /// <param name="partitionKey">Entity Partition Key</param>
        /// <param name="rowKey">Entity Row Key.</param>
        /// <typeparam name="TEntity">Type of Entity.</typeparam>
        /// <returns>Entity, or null if not found. A <see cref="Task" /> representing the asynchronous operation.</returns>
        [UsedImplicitly]
        public async Task<TEntity> RetreiveAsync<TEntity>(string partitionKey, string rowKey)
            where TEntity : TableEntity
        {
            var retrieveOperation = TableOperation.Retrieve<TEntity>(partitionKey, rowKey);
            var retrievedResult = await GetTable<TEntity>().ExecuteAsync(retrieveOperation);
            return (TEntity)retrievedResult?.Result;
        }

        /// <summary>
        /// Gets entity by pattern entity with partition and row keys.
        /// </summary>
        /// <param name="entity">Entity with partition and row key to use as pattern.</param>
        /// <typeparam name="TEntity">Type of Entity.</typeparam>
        /// <returns>Entity, or null if not found. A <see cref="Task" /> representing the asynchronous operation.</returns>
        [UsedImplicitly]
        public Task<TEntity> RetreiveAsync<TEntity>(TEntity entity)
            where TEntity : TableEntity
        {
            var attr = GetEntityAttribute<TEntity>();
            var partition = entity.PartitionKey ?? attr.PartitionKey;
            var key = entity.RowKey ?? attr.RowKey;
            return RetreiveAsync<TEntity>(partition, key);
        }

        /// <summary>
        /// Gets entity by row keys. Partition should be fixed in Table attribute.
        /// </summary>
        /// <param name="rowKey">Entity Row Key.</param>
        /// <typeparam name="TEntity">Type of Entity.</typeparam>
        /// <returns>Entity, or null if not found. A <see cref="Task" /> representing the asynchronous operation.</returns>
        [UsedImplicitly]
        public Task<TEntity> RetreiveAsync<TEntity>(string rowKey)
            where TEntity : TableEntity
        {
            return RetreiveAsync<TEntity>(GetEntityPartiton_<TEntity>(), rowKey);
        }

        /// <summary>
        /// Gets singleton entity. Partition and row keys should be fixed in Table attribute.
        /// </summary>
        /// <typeparam name="TEntity">Type of Entity.</typeparam>
        /// <returns>Entity, or null if not found. A <see cref="Task" /> representing the asynchronous operation.</returns>
        [UsedImplicitly]
        public Task<TEntity> RetreiveAsync<TEntity>()
            where TEntity : TableEntity
        {
            var attr = GetEntityAttribute<TEntity>();
            return RetreiveAsync<TEntity>(attr.PartitionKey, attr.RowKey);
        }

        /// <summary>
        /// Gets entity by pattern entity with partition and row keys.
        /// If entity does not exists it stores pattern entityt and returns it as result.
        /// </summary>
        /// <param name="entity">Entity with partition and row key to use as pattern.</param>
        /// <typeparam name="TEntity">Type of Entity.</typeparam>
        /// <returns>
        /// Entity from Table, or patter entity if not found. A <see cref="Task" /> representing the asynchronous
        /// operation.
        /// </returns>
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

        private static string GetEntityTable<T>()
        {
            return GetEntityAttribute<T>().Table;
        }

        private static TableQuery<TEntity> Query<TEntity>(TEntity entity)
            where TEntity : TableEntity, new()
        {
            var attr = GetEntityAttribute<TEntity>();
            var query = new TableQuery<TEntity>();
            if (attr.PartitionKey != null && attr.RowKey == null && entity.RowKey != null)
            {
                return query.Where(i => i.PartitionKey == attr.PartitionKey && i.RowKey == entity.RowKey);
            }

            if (attr.PartitionKey == null && attr.RowKey != null && entity.PartitionKey != null)
            {
                return query.Where(i => i.RowKey == attr.RowKey && i.PartitionKey == entity.PartitionKey);
            }

            return Query<TEntity>();
        }

        private static TableQuery<TEntity> Query<TEntity>()
            where TEntity : TableEntity, new()
        {
            var attr = GetEntityAttribute<TEntity>();

            var query = new TableQuery<TEntity>();
            if (attr.PartitionKey != null && attr.RowKey != null)
            {
                query = query.Where(i => i.PartitionKey == attr.PartitionKey && i.RowKey == attr.RowKey);
            }
            else if (attr.PartitionKey != null && attr.RowKey == null)
            {
                query = query.Where(i => i.PartitionKey == attr.PartitionKey);
            }
            else if (attr.PartitionKey == null && attr.RowKey != null)
            {
                query = query.Where(i => i.RowKey == attr.RowKey);
            }

            return query;
        }

        private string GetEntityKey_<T>()
        {
            return GetEntityAttribute<T>().RowKey ??
                   throw new ArgumentException($"Type {typeof(T)} does not have default Key");
        }

        private string GetEntityPartiton_<T>()
        {
            return GetEntityAttribute<T>().PartitionKey ??
                   throw new ArgumentException($"Type {typeof(T)} does not have default Partition");
        }

        private async Task<CloudTable> GetOrCreateTableAsync<T>()
        {
            var result = TableClient.GetTableReference(GetEntityTable<T>());
            await result.CreateIfNotExistsAsync();
            return result;
        }

        private void PrepareEntity<TEntity>(TEntity entity)
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

        private void ProcessResult(TableResult result)
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
    }
}