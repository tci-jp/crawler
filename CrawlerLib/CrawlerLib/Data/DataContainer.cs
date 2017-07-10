// <copyright file="DataContainer.cs" company="DECTech.Tokyo">
// Copyright (c) DECTech.Tokyo. All rights reserved.
// </copyright>

namespace CrawlerLib
{
    using System;
    using System.Linq;

    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Net;
    using System.Reflection;
    using System.Threading.Tasks;
    using Microsoft.Azure;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Blob;
    using Microsoft.WindowsAzure.Storage.Table;
    using System.Configuration;

    using System;

    public class DataStorage
    {
        private readonly ConcurrentDictionary<Type, TableAttribute> typeToAttribute = new ConcurrentDictionary<Type, TableAttribute>();

        public CloudBlobClient BlobClient { get; }

        private CloudStorageAccount StorageAccount { get; }

        // ConfigurationManager.AppSettings["StorageConnectionString"]
        private CloudTableClient TableClient { get; }

        public DataStorage(string connectionString)
        {
            StorageAccount = CloudStorageAccount.Parse(connectionString);
            TableClient = StorageAccount.CreateCloudTableClient();
            BlobClient = StorageAccount.CreateCloudBlobClient();
        }

        public IQueryable<TEntity> Query<TEntity>()
            where TEntity : TableEntity, new()
        {
            var table = GetTable<TEntity>();
            var attr = GetEntityAttribute<TEntity>();

            var query = table.CreateQuery<TEntity>();
            if (attr.Partition != null && attr.Key != null)
            {
                return query.Where(i => i.PartitionKey == attr.Partition && i.RowKey == attr.Key);
            }

            if (attr.Partition != null && attr.Key == null)
            {
                return query.Where(i => i.PartitionKey == attr.Partition);
            }

            if (attr.Partition == null && attr.Key != null)
            {
                return query.Where(i => i.RowKey == attr.Key);
            }

            return query;
        }

        public async Task<CloudBlobContainer> GetBlobContainer(string name)
        {
            var container = BlobClient.GetContainerReference(name);
            await container.CreateIfNotExistsAsync();
            return container;
        }

        private void PrepareEntity<TEntity>(TEntity entity)
            where TEntity : TableEntity
        {
            var attr = GetEntityAttribute<TEntity>();
            if (entity.PartitionKey == null)
            {
                entity.PartitionKey = attr.Partition;
            }

            if (entity.RowKey == null)
            {
                entity.RowKey = attr.Key;
            }
        }

        public async Task ReplaceAsync<TEntity>(TEntity entity)
            where TEntity : TableEntity
        {
            PrepareEntity(entity);
            ProcessResult(await (await GetOrCreateTableAsync<TEntity>()).ExecuteAsync(TableOperation.Replace(entity)));
        }

        public async Task InsertAsync<TEntity>(TEntity entity)
            where TEntity : TableEntity
        {
            PrepareEntity(entity);
            ProcessResult(await (await GetOrCreateTableAsync<TEntity>()).ExecuteAsync(TableOperation.Insert(entity)));
        }

        public async Task InsertOrReplaceAsync<TEntity>(TEntity entity)
            where TEntity : TableEntity
        {
            PrepareEntity(entity);
            ProcessResult(await (await GetOrCreateTableAsync<TEntity>()).ExecuteAsync(TableOperation.InsertOrReplace(entity)));
        }

        public async Task<TEntity> RetreiveAsync<TEntity>(string partition, string key)
            where TEntity : TableEntity
        {
            var retrieveOperation = TableOperation.Retrieve<TEntity>(partition, key);
            var retrievedResult = await GetTable<TEntity>().ExecuteAsync(retrieveOperation);
            return (TEntity)retrievedResult?.Result;
        }

        public Task<TEntity> RetreiveAsync<TEntity>(TEntity entity)
            where TEntity : TableEntity
        {
            var attr = GetEntityAttribute<TEntity>();
            var partition = entity.PartitionKey ?? attr.Partition;
            var key = entity.RowKey ?? attr.Key;
            return RetreiveAsync<TEntity>(partition, key);
        }

        public async Task<bool> DeleteAsync<TEntity>(TEntity entity)
            where TEntity : TableEntity
        {
            var retrieveOperation = TableOperation.Delete(entity);
            var retrievedResult = await GetTable<TEntity>().ExecuteAsync(retrieveOperation);
            return retrievedResult.Http​Status​Code == 200;
        }

        public Task<TEntity> RetreiveAsync<TEntity>(string key)
            where TEntity : TableEntity
        {
            return RetreiveAsync<TEntity>(GetEntityPartiton_<TEntity>(), key);
        }

        public Task<TEntity> RetreiveAsync<TEntity>()
            where TEntity : TableEntity
        {
            var attr = GetEntityAttribute<TEntity>();
            return RetreiveAsync<TEntity>(attr.Partition, attr.Key);
        }

        public async Task<TEntity> RetreiveOrCreateAsync<TEntity>(TEntity entity)
                            where TEntity : TableEntity
        {
            var attr = GetEntityAttribute<TEntity>();
            var partition = entity.PartitionKey ?? attr.Partition;
            var key = entity.RowKey ?? attr.Key;
            return await RetreiveAsync<TEntity>(partition, key) ?? entity;
        }

        private TableAttribute GetEntityAttribute<T>()
        {
            return typeToAttribute.GetOrAdd(typeof(T), t =>
            {
                var attr = t.GetCustomAttributes(typeof(TableAttribute), true).SingleOrDefault() as TableAttribute;
                if (attr == null)
                {
                    throw new ArgumentException($"Type {typeof(T)} does not have Table attribute");
                }
                return attr;
            });
        }

        private string GetEntityKey_<T>()
        {
            return GetEntityAttribute<T>().Key ?? throw new ArgumentException($"Type {typeof(T)} does not have default Key");
        }

        private string GetEntityPartiton_<T>()
        {
            return GetEntityAttribute<T>().Partition ?? throw new ArgumentException($"Type {typeof(T)} does not have default Partition");
        }

        private string GetEntityTable<T>()
        {
            return GetEntityAttribute<T>().Table;
        }

        private async Task<CloudTable> GetOrCreateTableAsync<T>()
        {
            var result = TableClient.GetTableReference(GetEntityTable<T>());
            await result.CreateIfNotExistsAsync();
            return result;
        }

        public CloudTable GetTable<T>()
        {
            var result = TableClient.GetTableReference(GetEntityTable<T>());
            return result;
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

        public IQueryable<TEntity> Query<TEntity>(Expression<Func<TEntity, bool>> func)
            where TEntity : TableEntity, new()
        {
            var visitor = new PropertyReplacer<TEntity>();
            var newfunc = visitor.VisitAndConvert(func);

            var result = Query<TEntity>().Where(newfunc);
            return result;
        }

        private class PropertyReplacer<TEntity> : ExpressionVisitor
        {
            public Expression<Func<TEntity, bool>> VisitAndConvert(Expression<Func<TEntity, bool>> root)
            {
                return (Expression<Func<TEntity, bool>>)VisitLambda(root);
            }

            protected override Expression VisitBinary(BinaryExpression node)
            {
                if (node.Left is UnaryExpression left && IsEnumConvert(left) && node.Right.Type == typeof(int))
                {
                    var newMember = ReplaceMember(left.Operand as MemberExpression);
                    if (newMember != null)
                    {
                        return Expression.MakeBinary(
                            node.NodeType,
                            newMember,
                            GetEnumString(node.Right, left.Operand.Type));
                    }
                }

                if (node.Right is UnaryExpression right && IsEnumConvert(right) && node.Left.Type == typeof(int))
                {
                    var newMember = ReplaceMember(right.Operand as MemberExpression);
                    if (newMember != null)
                    {
                        return Expression.MakeBinary(
                            node.NodeType,
                            GetEnumString(node.Left, right.Operand.Type),
                            newMember);
                    }
                }

                return base.VisitBinary(node);
            }

            private Expression GetEnumString(Expression node, Type type)
            {
                return Expression.Call(Expression.Convert(node, type), "ToString", new Type[0]);
            }

            private bool IsEnumConvert(UnaryExpression node) => node?.NodeType == ExpressionType.Convert && node.Operand.Type.IsEnum;

            private MemberExpression ReplaceMember(MemberExpression node)
            {
                if (node == null)
                {
                    return null;
                }

                if (node.Member.GetCustomAttribute(typeof(RowKeyAttribute)) != null)
                {
                    return Expression.MakeMemberAccess(node.Expression, typeof(TableEntity).GetMember("RowKey").Single());
                }

                if (node.Member.GetCustomAttribute(typeof(PartitionKeyAttribute)) != null)
                {
                    return Expression.MakeMemberAccess(node.Expression, typeof(TableEntity).GetMember("PartitionKey").Single());
                }

                return null;
            }

            protected override Expression VisitMember(MemberExpression node)
            {
                return ReplaceMember(node) ?? base.VisitMember(node);
            }
        }
    }
}
