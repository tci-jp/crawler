// <copyright file="PropertyReplacer.cs" company="DECTech.Tokyo">
// Copyright (c) DECTech.Tokyo. All rights reserved.
// </copyright>

namespace Azure.Storage
{
    using System;
    using System.Linq.Expressions;
    using System.Reflection;
    using Microsoft.WindowsAzure.Storage.Table;

    /// <inheritdoc />
    /// <summary>
    /// Rewrites expression to use RowKey and PartitionKey instead of mapping get-properties.
    /// </summary>
    /// <typeparam name="TEntity">Table entity</typeparam>
    internal class PropertyReplacer<TEntity> : ExpressionVisitor
    {
        /// <summary>
        /// Start convertion.
        /// </summary>
        /// <param name="root">Expression to convert.</param>
        /// <returns>Converted expression.</returns>
        public Expression<Func<TEntity, bool>> VisitAndConvert(Expression<Func<TEntity, bool>> root)
        {
            return (Expression<Func<TEntity, bool>>)VisitLambda(root);
        }

        /// <inheritdoc />
        protected override Expression VisitBinary(BinaryExpression node)
        {
            if (node.Left is UnaryExpression left && IsEnumConvert(left) && (node.Right.Type == typeof(int)))
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

            if (node.Right is UnaryExpression right && IsEnumConvert(right) && (node.Left.Type == typeof(int)))
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

        /// <inheritdoc />
        protected override Expression VisitMember(MemberExpression node)
        {
            return ReplaceMember(node) ?? base.VisitMember(node);
        }

        private static Expression GetEnumString(Expression node, Type type)
        {
            return Expression.Call(Expression.Convert(node, type), "ToString", new Type[0]);
        }

        private static bool IsEnumConvert(UnaryExpression node)
        {
            return (node?.NodeType == ExpressionType.Convert) && node.Operand.Type.GetTypeInfo().IsEnum;
        }

        private static MemberExpression ReplaceMember(MemberExpression node)
        {
            if (node == null)
            {
                return null;
            }

            if (node.Member.GetCustomAttribute(typeof(RowKeyAttribute)) != null)
            {
                return Expression.MakeMemberAccess(
                    node.Expression,
                    typeof(TableEntity).GetRuntimeProperty("RowKey"));
            }

            if (node.Member.GetCustomAttribute(typeof(PartitionKeyAttribute)) != null)
            {
                return Expression.MakeMemberAccess(
                    node.Expression,
                    typeof(TableEntity).GetRuntimeProperty("PartitionKey"));
            }

            return null;
        }
    }
}