// <copyright file="DataStorage.cs" company="DECTech.Tokyo">
// Copyright (c) DECTech.Tokyo. All rights reserved.
// </copyright>

namespace Azure.Storage
{
    using System;
    using System.Collections.Concurrent;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Net;
    using System.Reflection;
    using System.Text;
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
        private readonly ConcurrentDictionary<Type, TableAttribute> typeToAttribute =
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

        // ConfigurationManager.AppSettings["StorageConnectionString"]
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
        /// Deletes Table entity with specific PartitionKey and RowKey.
        /// </summary>
        /// <param name="entity">Entity to delete.</param>
        /// <typeparam name="TEntity">Type of Table entity</typeparam>
        /// <returns></returns>
        [UsedImplicitly]
        public async Task<bool> DeleteAsync<TEntity>(TEntity entity)
            where TEntity : TableEntity
        {
            var retrieveOperation = TableOperation.Delete(entity);
            var retrievedResult = await GetTable<TEntity>().ExecuteAsync(retrieveOperation);
            return retrievedResult.HttpStatusCode == 200;
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

        [UsedImplicitly]
        public async Task<CloudBlobContainer> GetBlobContainer(string name)
        {
            var container = BlobClient.GetContainerReference(name);
            await container.CreateIfNotExistsAsync();
            return container;
        }

        [UsedImplicitly]
        public CloudTable GetTable<T>()
        {
            var result = TableClient.GetTableReference(GetEntityTable<T>());
            return result;
        }

        [UsedImplicitly]
        public async Task InsertAsync<TEntity>(TEntity entity)
            where TEntity : TableEntity
        {
            PrepareEntity(entity);
            var cloudTable = await GetOrCreateTableAsync<TEntity>();
            ProcessResult(await cloudTable.ExecuteAsync(TableOperation.Insert(entity)));
        }

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
        /// <returns>Queryable for further quering.</returns>
        [UsedImplicitly]
        public IQueryable<TEntity> Query<TEntity>()
            where TEntity : TableEntity, new()
        {
            var table = GetTable<TEntity>();
            var attr = GetEntityAttribute<TEntity>();

            var query = table.CreateQuery<TEntity>();
            if (attr.PartitionKey != null && attr.RowKey != null)
            {
                return query.Where(i => i.PartitionKey == attr.PartitionKey && i.RowKey == attr.RowKey);
            }

            if (attr.PartitionKey != null && attr.RowKey == null)
            {
                return query.Where(i => i.PartitionKey == attr.PartitionKey);
            }

            if (attr.PartitionKey == null && attr.RowKey != null)
            {
                return query.Where(i => i.RowKey == attr.RowKey);
            }

            return query;
        }

        /// <summary>
        /// Starts query for specific Table type.
        /// </summary>
        /// <typeparam name="TEntity">Type of Table.</typeparam>
        /// <param name="entity">Entity with not null PartitionKey or RowKey for query</param>
        /// <returns>Queryable for further quering.</returns>
        [UsedImplicitly]
        public IQueryable<TEntity> Query<TEntity>([NotNull] TEntity entity)
            where TEntity : TableEntity, new()
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            var table = GetTable<TEntity>();
            var attr = GetEntityAttribute<TEntity>();

            var query = table.CreateQuery<TEntity>();
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

        /// <summary>
        /// Query Azure Table by expression
        /// </summary>
        /// <param name="func">Query expression.</param>
        /// <typeparam name="TEntity">Type of Entity.</typeparam>
        /// <returns>Queryable with result.</returns>
        [UsedImplicitly]
        public IQueryable<TEntity> Query<TEntity>(Expression<Func<TEntity, bool>> func)
            where TEntity : TableEntity, new()
        {
            var visitor = new PropertyReplacer<TEntity>();
            var newfunc = visitor.VisitAndConvert(func);

            var result = Query<TEntity>().Where(newfunc);
            return result;
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

        [UsedImplicitly]
        public async Task<TEntity> RetreiveAsync<TEntity>(string partition, string key)
            where TEntity : TableEntity
        {
            var retrieveOperation = TableOperation.Retrieve<TEntity>(partition, key);
            var retrievedResult = await GetTable<TEntity>().ExecuteAsync(retrieveOperation);
            return (TEntity)retrievedResult?.Result;
        }

        [UsedImplicitly]
        public Task<TEntity> RetreiveAsync<TEntity>(TEntity entity)
            where TEntity : TableEntity
        {
            var attr = GetEntityAttribute<TEntity>();
            var partition = entity.PartitionKey ?? attr.PartitionKey;
            var key = entity.RowKey ?? attr.RowKey;
            return RetreiveAsync<TEntity>(partition, key);
        }

        [UsedImplicitly]
        public Task<TEntity> RetreiveAsync<TEntity>(string key)
            where TEntity : TableEntity
        {
            return RetreiveAsync<TEntity>(GetEntityPartiton_<TEntity>(), key);
        }

        [UsedImplicitly]
        public Task<TEntity> RetreiveAsync<TEntity>()
            where TEntity : TableEntity
        {
            var attr = GetEntityAttribute<TEntity>();
            return RetreiveAsync<TEntity>(attr.PartitionKey, attr.RowKey);
        }

        [UsedImplicitly]
        public async Task<TEntity> RetreiveOrCreateAsync<TEntity>(TEntity entity)
            where TEntity : TableEntity
        {
            var attr = GetEntityAttribute<TEntity>();
            var partition = entity.PartitionKey ?? attr.PartitionKey;
            var key = entity.RowKey ?? attr.RowKey;
            return await RetreiveAsync<TEntity>(partition, key) ?? entity;
        }

        private TableAttribute GetEntityAttribute<T>()
        {
            return typeToAttribute.GetOrAdd(typeof(T), t =>
            {
                var attr =
                    t.GetCustomAttributes(typeof(TableAttribute), true)
                     .SingleOrDefault() as TableAttribute;
                if (attr == null)
                {
                    throw new ArgumentException(
                        $"Type {typeof(T)} does not have Table attribute");
                }
                return attr;
            });
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

            protected override Expression VisitMember(MemberExpression node)
            {
                return ReplaceMember(node) ?? base.VisitMember(node);
            }

            private Expression GetEnumString(Expression node, Type type)
            {
                return Expression.Call(Expression.Convert(node, type), "ToString", new Type[0]);
            }

            private bool IsEnumConvert(UnaryExpression node)
            {
                return node?.NodeType == ExpressionType.Convert && node.Operand.Type.IsEnum;
            }

            private MemberExpression ReplaceMember(MemberExpression node)
            {
                if (node == null)
                {
                    return null;
                }

                if (node.Member.GetCustomAttribute(typeof(RowKeyAttribute)) != null)
                {
                    return Expression.MakeMemberAccess(
                        node.Expression,
                        typeof(TableEntity).GetMember("RowKey").Single());
                }

                if (node.Member.GetCustomAttribute(typeof(PartitionKeyAttribute)) != null)
                {
                    return Expression.MakeMemberAccess(
                        node.Expression,
                        typeof(TableEntity).GetMember("PartitionKey").Single());
                }

                return null;
            }
        }
    }
}