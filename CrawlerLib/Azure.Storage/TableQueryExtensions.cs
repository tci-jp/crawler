// <copyright file="TableQueryExtensions.cs" company="DECTech.Tokyo">
// Copyright (c) DECTech.Tokyo. All rights reserved.
// </copyright>

namespace Azure.Storage
{
    using System;
    using System.Linq.Expressions;
    using Microsoft.WindowsAzure.Storage.Table;

    /// <summary>
    /// Extensions for TableQuery
    /// </summary>
    public static class TableQueryExtensions
    {
        /// <summary>
        /// Add conditions for query
        /// </summary>
        /// <typeparam name="TEntity">Table entity.</typeparam>
        /// <param name="query">Query object.</param>
        /// <param name="expression">Condition expression.</param>
        /// <returns>Updated query.</returns>
        public static TableQuery<TEntity> Where<TEntity>(
            this TableQuery<TEntity> query,
            Expression<Func<TEntity, bool>> expression)
            where TEntity : TableEntity, new()
        {
            var querystring = ConvertTop(expression.Body);
            return query.Where(querystring);
        }

        private static string CombineBinary(string op, BinaryExpression parameters)
        {
            return TableQuery.CombineFilters(ConvertTop(parameters.Left), op, ConvertTop(parameters.Right));
        }

        private static string CombineNot(UnaryExpression parameter)
        {
            return $"not ({ConvertTop(parameter.Operand)})";
        }

        private static string ConvertBinary(string op, BinaryExpression expression)
        {
            var left = GetGettingProperty(expression.Left);
            if (left != null)
            {
                var value = Expression.Lambda(expression.Right).Compile().DynamicInvoke();
                return GenerateFilter(left, op, value);
            }

            var right = GetGettingProperty(expression.Right);
            if (right != null)
            {
                var value = Expression.Lambda(expression.Left).Compile().DynamicInvoke();
                return GenerateFilter(right, InvertOp(op), value);
            }

            throw new InvalidOperationException("Must be comparison with TableEntity property.");
        }

        private static string ConvertNext(Expression expression)
        {
            string op;
            switch (expression.NodeType)
            {
                case ExpressionType.Equal:
                    op = "eq";
                    break;
                case ExpressionType.GreaterThan:
                    op = "gt";
                    break;
                case ExpressionType.GreaterThanOrEqual:
                    op = "ge";
                    break;
                case ExpressionType.LessThan:
                    op = "lt";
                    break;
                case ExpressionType.LessThanOrEqual:
                    op = "le";
                    break;
                case ExpressionType.NotEqual:
                    op = "ne";
                    break;
                default:
                    throw new InvalidOperationException("Not supported second level expression: " +
                                                        expression.NodeType);
            }

            return ConvertBinary(op, (BinaryExpression)expression);
        }

        private static string ConvertTop(Expression expression)
        {
            switch (expression.NodeType)
            {
                case ExpressionType.AndAlso:
                    return CombineBinary("and", (BinaryExpression)expression);
                case ExpressionType.Not:
                    return CombineNot((UnaryExpression)expression);
                case ExpressionType.OrElse:
                    return CombineBinary("or", (BinaryExpression)expression);
                case ExpressionType.Equal:
                case ExpressionType.GreaterThan:
                case ExpressionType.GreaterThanOrEqual:
                case ExpressionType.LessThan:
                case ExpressionType.LessThanOrEqual:
                case ExpressionType.NotEqual:
                    return ConvertNext(expression);
                default:
                    throw new InvalidOperationException("Operation is not supported: " + expression.NodeType);
            }
        }

        private static string GenerateFilter(string left, string op, object value)
        {
            switch (value)
            {
                case string s:
                    return TableQuery.GenerateFilterCondition(left, op, s);
                case bool b:
                    return TableQuery.GenerateFilterConditionForBool(left, op, b);
                case DateTime d:
                    return TableQuery.GenerateFilterConditionForDate(left, op, d);
                case DateTimeOffset d:
                    return TableQuery.GenerateFilterConditionForDate(left, op, d);
                case double d:
                    return TableQuery.GenerateFilterConditionForDouble(left, op, d);
                case float f:
                    return TableQuery.GenerateFilterConditionForDouble(left, op, f);
                case byte i:
                    return TableQuery.GenerateFilterConditionForInt(left, op, i);
                case short i:
                    return TableQuery.GenerateFilterConditionForInt(left, op, i);
                case int i:
                    return TableQuery.GenerateFilterConditionForInt(left, op, i);
                case long l:
                    return TableQuery.GenerateFilterConditionForLong(left, op, l);
                default:
                    throw new InvalidOperationException("Type is not supported: " + value.GetType());
            }
        }

        private static string GetGettingProperty(Expression expr)
        {
            if (expr is MemberExpression memb)
            {
                if (memb.Expression is ParameterExpression)
                {
                    return memb.Member.Name;
                }
            }

            return null;
        }

        private static string InvertOp(string op)
        {
            switch (op)
            {
                case "gt": return "lt";
                case "lt": return "gt";
                case "ge": return "le";
                case "le": return "ge";
                default: return op;
            }
        }
    }
}