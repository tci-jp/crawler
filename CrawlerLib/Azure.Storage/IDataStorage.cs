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

    public interface IDataStorage
    {
        CloudBlobClient BlobClient { get; }

        Task<bool> DeleteAsync<TEntity>(TEntity entity)
            where TEntity : TableEntity;

        IAsyncEnumerable<TEntity> ExecuteQuery<TEntity>(
            TableQuery<TEntity> query,
            CancellationToken token = default(CancellationToken))
            where TEntity : TableEntity, new();

        Task<CloudBlobContainer> GetBlobContainerAsync(string name);

        CloudTable GetTable<T>();

        Task InsertAsync<TEntity>(TEntity entity)
            where TEntity : TableEntity;

        Task InsertOrReplaceAsync<TEntity>(TEntity entity)
            where TEntity : TableEntity;

        IAsyncEnumerable<TEntity> QueryAsync<TEntity>(CancellationToken token = default(CancellationToken))
            where TEntity : TableEntity, new();

        IAsyncEnumerable<TEntity> QueryAsync<TEntity>(
            Expression<Func<TEntity, bool>> func,
            CancellationToken token = default(CancellationToken))
            where TEntity : TableEntity, new();

        IAsyncEnumerable<TEntity> QueryAsync<TEntity>(
            [NotNull] TEntity entity,
            CancellationToken token = default(CancellationToken))
            where TEntity : TableEntity, new();

        Task<TableQuerySegment<TEntity>> QuerySegmentedAsync<TEntity>(
            Expression<Func<TEntity, bool>> func,
            int segmentSize = 10,
            TableContinuationToken token = null,
            CancellationToken cancellation =
                default(CancellationToken))
            where TEntity : TableEntity, new();

        Task ReplaceAsync<TEntity>(TEntity entity)
            where TEntity : TableEntity;

        Task<TEntity> RetreiveAsync<TEntity>()
            where TEntity : TableEntity;

        Task<TEntity> RetreiveAsync<TEntity>(string rowKey)
            where TEntity : TableEntity;

        Task<TEntity> RetreiveAsync<TEntity>(string partitionKey, string rowKey)
            where TEntity : TableEntity;

        Task<TEntity> RetreiveAsync<TEntity>(TEntity entity)
            where TEntity : TableEntity;

        Task<TEntity> RetreiveOrCreateAsync<TEntity>(TEntity entity)
            where TEntity : TableEntity;
    }
}