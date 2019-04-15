using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Abitech.NextApi.Model.Filtering;
using Microsoft.EntityFrameworkCore.Internal;

namespace Abitech.NextApi.Server.Entity
{
    /// <summary>
    /// Filter extensions
    /// </summary>
    public static class FilterExtensions
    {
        private static readonly MethodInfo StringContainsMethod =
            typeof(string).GetMethod("Contains", new[] {typeof(string)});

        // thx to: https://www.codeproject.com/Articles/1079028/Build-Lambda-Expressions-Dynamically

        /// <summary>
        /// Convert filter to lambda filter
        /// </summary>
        /// <param name="filter"></param>
        /// <typeparam name="TEntity"></typeparam>
        /// <returns></returns>
        public static Expression<Func<TEntity, bool>> ToLambdaFilter<TEntity>(this Filter filter)
            where TEntity : class
        {
            if (filter?.Expressions == null || !EnumerableExtensions.Any(filter.Expressions))
            {
                return null;
            }

            var parameter = Expression.Parameter(typeof(TEntity), "entity");
            var allExpressions = BuildExpression(parameter, filter);

            return Expression.Lambda<Func<TEntity, bool>>(allExpressions, parameter);
        }

        private static Expression BuildExpression(ParameterExpression parameter, Filter filter)
        {
            Expression allExpressions = null;
            foreach (var filterExpression in filter.Expressions)
            {
                var property = GetMemberExpression(parameter, filterExpression.Property);
                Expression currentExpression;
                switch (filterExpression.ExpressionType)
                {
                    case FilterExpressionTypes.Contains:
                        currentExpression = Expression.Call(property, StringContainsMethod,
                            Expression.Constant((string)filterExpression.Value));
                        break;
                    case FilterExpressionTypes.Equal:
                        currentExpression = Expression.Equal(property,
                            Expression.Constant(filterExpression.Value));
                        break;
                    case FilterExpressionTypes.MoreThan:
                        currentExpression =
                            Expression.GreaterThan(property,
                                Expression.Constant(filterExpression.Value));
                        break;
                    case FilterExpressionTypes.LessThan:
                        currentExpression =
                            Expression.LessThan(property,
                                Expression.Constant(filterExpression.Value));
                        break;
                    case FilterExpressionTypes.MoreThanOrEqual:
                        currentExpression =
                            Expression.GreaterThanOrEqual(property,
                                Expression.Constant(filterExpression.Value));
                        break;
                    case FilterExpressionTypes.LessThanOrEqual:
                        currentExpression =
                            Expression.LessThanOrEqual(property,
                                Expression.Constant(filterExpression.Value));
                        break;
                    case FilterExpressionTypes.In:
                        var inputArray = (ICollection)filterExpression.Value;
                        var items = (from object item
                                    in inputArray
                                select Expression
                                    .Constant(item))
                            .Cast<Expression>()
                            .ToList();
                        var itemType = items.First().Type;
                        var arrayExpression = Expression.NewArrayInit(itemType, items);
                        var containsMethod = typeof(ICollection<>).MakeGenericType(itemType).GetMethod("Contains");
                        currentExpression =
                            Expression.Call(arrayExpression, containsMethod
                                , property);
                        break;
                    case FilterExpressionTypes.Filter:
                        currentExpression = BuildExpression(parameter, (Filter)filterExpression.Value);
                        break;
                    case FilterExpressionTypes.NotEqual:
                        currentExpression =
                            Expression.NotEqual(property,
                                Expression.Constant(filterExpression.Value));
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                if (currentExpression != null)
                    allExpressions = allExpressions == null
                        ? currentExpression
                        : filter.LogicalOperator == LogicalOperators.And
                            ? Expression.AndAlso(allExpressions, currentExpression)
                            : Expression.OrElse(allExpressions, currentExpression);
            }

            return allExpressions;
        }

        private static MemberExpression GetMemberExpression(Expression param, string propertyName)
        {
            // member expression navigation memberA.memberB.memberC
            while (true)
            {
                if (propertyName == null) return null;
                if (!propertyName.Contains("."))
                {
                    return Expression.Property(param, propertyName);
                }

                var index = propertyName.IndexOf(".", StringComparison.Ordinal);
                var subParam = Expression.Property(param, propertyName.Substring(0, index));
                param = subParam;
                propertyName = propertyName.Substring(index + 1);
            }
        }
    }
}
