// <copyright file="IDataStorage.cs" company="DECTech.Tokyo">
// Copyright (c) DECTech.Tokyo. All rights reserved.
// </copyright>

namespace Azure.Storage
{
    using System;
    using System.Collections.Async;
    using System.Linq.Expressions;
    using System.Threading;
    using System.Threading.Tasks;
    using JetBrains.Annotations;
    using Microsoft.WindowsAzure.Storage.Blob;
    using Microsoft.WindowsAzure.Storage.Table;

    /// <summary>
    /// Base interface for storage mocking.
    /// </summary>
    public interface IDataStorage
    {
        /// <summary>
        /// Gets Azure Cloud Blob Client
        /// </summary>
        CloudBlobClient BlobClient { get; }

        /// <summary>
        /// Deletes Table entity with specific PartitionKey and RowKey.
        /// </summary>
        /// <param name="entity">Entity to delete.</param>
        /// <typeparam name="TEntity">Type of Table entity</typeparam>
        /// <returns>A <see cref="Task" /> representing the asynchronous operation.</returns>
        Task<bool> DeleteAsync<TEntity>(TEntity entity)
            where TEntity : TableEntity;

        /// <summary>
        /// Execute Table query
        /// </summary>
        /// <typeparam name="TEntity">Table entity for query.</typeparam>
        /// <param name="query">Query.</param>
        /// <param name="token">Cancellation.</param>
        /// <returns>Async collection of entitoes.</returns>
        IAsyncEnumerable<TEntity> ExecuteQuery<TEntity>(
            TableQuery<TEntity> query,
            CancellationToken token = default(CancellationToken))
            where TEntity : TableEntity, new();

        /// <summary>
        /// Gets Blob Container for this Azure Storage.
        /// </summary>
        /// <param name="name">Blob Container name.</param>
        /// <returns>Blob Container object. A <see cref="Task" /> representing the asynchronous operation.</returns>
        Task<CloudBlobContainer> GetBlobContainerAsync(string name);

        /// <summary>
        /// Gets Table object by type.
        /// </summary>
        /// <typeparam name="T">Type of Table.</typeparam>
        /// <returns>Table object.</returns>
        CloudTable GetTable<T>();

        /// <summary>
        /// Inserts new entity into table. No existing entity with the same Row and Partition keys is allowed.
        /// </summary>
        /// <param name="entity">Entity to insert.</param>
        /// <typeparam name="TEntity">Entity type.</typeparam>
        /// <returns>A <see cref="Task" /> representing the asynchronous operation.</returns>
        Task InsertAsync<TEntity>(TEntity entity)
            where TEntity : TableEntity;

        /// <summary>
        /// Inserts entity into table. If entity with the same Row and Partition keys exists it get replaced.
        /// </summary>
        /// <param name="entity">Entity to insert.</param>
        /// <typeparam name="TEntity">Entity type.</typeparam>
        /// <returns>A <see cref="Task" /> representing the asynchronous operation.</returns>
        Task InsertOrReplaceAsync<TEntity>(TEntity entity)
            where TEntity : TableEntity;

        /// <summary>
        /// Starts query for specific Table type.
        /// </summary>
        /// <typeparam name="TEntity">Type of Table.</typeparam>
        /// <param name="token">Cancellation</param>
        /// <returns>Queryable for further quering.</returns>
        IAsyncEnumerable<TEntity> QueryAsync<TEntity>(CancellationToken token = default(CancellationToken))
            where TEntity : TableEntity, new();

        /// <summary>
        /// Query Azure Table by expression
        /// </summary>
        /// <param name="func">Query expression.</param>
        /// <typeparam name="TEntity">Type of Entity.</typeparam>
        /// <param name="token">Cancellation</param>
        /// <returns>Queryable with result.</returns>
        IAsyncEnumerable<TEntity> QueryAsync<TEntity>(
            Expression<Func<TEntity, bool>> func,
            CancellationToken token = default(CancellationToken))
            where TEntity : TableEntity, new();

        /// <summary>
        /// Starts query for specific Table type.
        /// </summary>
        /// <typeparam name="TEntity">Type of Table.</typeparam>
        /// <param name="entity">Entity with not null PartitionKey or RowKey for query</param>
        /// <param name="token">Cancellation</param>
        /// <returns>Queryable for further quering.</returns>
        IAsyncEnumerable<TEntity> QueryAsync<TEntity>(
            [NotNull] TEntity entity,
            CancellationToken token = default(CancellationToken))
            where TEntity : TableEntity, new();

        /// <summary>
        /// Query Azure Table by expression
        /// </summary>
        /// <param name="func">Query expression.</param>
        /// <typeparam name="TEntity">Type of Entity.</typeparam>
        /// <param name="segmentSize">Number of items in one segment</param>
        /// <param name="token">Continuation token.</param>
        /// <param name="cancellation">Cancellation</param>
        /// <returns>Queryable with result.</returns>
        Task<TableQuerySegment<TEntity>> QuerySegmentedAsync<TEntity>(
            Expression<Func<TEntity, bool>> func,
            int segmentSize = 10,
            TableContinuationToken token = null,
            CancellationToken cancellation =
                default(CancellationToken))
            where TEntity : TableEntity, new();

        /// <summary>
        /// Replace only if entity already exists.
        /// </summary>
        /// <param name="entity">Entity to replace.</param>
        /// <typeparam name="TEntity">Type of Entity.</typeparam>
        /// <returns>A <see cref="Task" /> representing the asynchronous operation.</returns>
        Task ReplaceAsync<TEntity>(TEntity entity)
            where TEntity : TableEntity;

        /// <summary>
        /// Gets singleton entity. Partition and row keys should be fixed in Table attribute.
        /// </summary>
        /// <typeparam name="TEntity">Type of Entity.</typeparam>
        /// <returns>Entity, or null if not found. A <see cref="Task" /> representing the asynchronous operation.</returns>
        Task<TEntity> RetreiveAsync<TEntity>()
            where TEntity : TableEntity;

        /// <summary>
        /// Gets entity by row keys. Partition should be fixed in Table attribute.
        /// </summary>
        /// <param name="rowKey">Entity Row Key.</param>
        /// <typeparam name="TEntity">Type of Entity.</typeparam>
        /// <returns>Entity, or null if not found. A <see cref="Task" /> representing the asynchronous operation.</returns>
        Task<TEntity> RetreiveAsync<TEntity>(string rowKey)
            where TEntity : TableEntity;

        /// <summary>
        /// Gets entity by partition and row keys.
        /// </summary>
        /// <param name="partitionKey">Entity Partition Key</param>
        /// <param name="rowKey">Entity Row Key.</param>
        /// <typeparam name="TEntity">Type of Entity.</typeparam>
        /// <returns>Entity, or null if not found. A <see cref="Task" /> representing the asynchronous operation.</returns>
        Task<TEntity> RetreiveAsync<TEntity>(string partitionKey, string rowKey)
            where TEntity : TableEntity;

        /// <summary>
        /// Gets entity by pattern entity with partition and row keys.
        /// </summary>
        /// <param name="entity">Entity with partition and row key to use as pattern.</param>
        /// <typeparam name="TEntity">Type of Entity.</typeparam>
        /// <returns>Entity, or null if not found. A <see cref="Task" /> representing the asynchronous operation.</returns>
        Task<TEntity> RetreiveAsync<TEntity>(TEntity entity)
            where TEntity : TableEntity;

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
        Task<TEntity> RetreiveOrCreateAsync<TEntity>(TEntity entity)
            where TEntity : TableEntity;
   }
}