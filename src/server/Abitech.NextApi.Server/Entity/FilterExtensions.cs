using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Abitech.NextApi.Model;
using Abitech.NextApi.Model.Filtering;
using Newtonsoft.Json.Linq;

namespace Abitech.NextApi.Server.Entity
{
    /// <summary>
    /// Filter extensions
    /// </summary>
    public static class FilterExtensions
    {
        private static readonly MethodInfo StringContainsMethod =
            typeof(string).GetMethod("Contains", new[] {typeof(string)});

        private static readonly MethodInfo ToStringMethod =
            typeof(object).GetMethod("ToString", Type.EmptyTypes);

        private static readonly MethodInfo ToLowerMethod =
            typeof(string).GetMethod("ToLower", Type.EmptyTypes);

        // more info: https://docs.microsoft.com/en-us/dotnet/api/system.linq.enumerable.any?view=netcore-2.2
        private static readonly MethodInfo Any = typeof(Enumerable).GetMethods()
            .First(m => m.Name == "Any" && m.GetParameters().Length == 1);

        private static readonly MethodInfo AnyWithPredicate = typeof(Enumerable).GetMethods()
            .First(m => m.Name == "Any" && m.GetParameters().Length == 2);

        // more info: https://docs.microsoft.com/en-us/dotnet/api/system.linq.enumerable.all?view=netcore-2.2
        private static readonly MethodInfo All = typeof(Enumerable).GetMethods()
            .First(m => m.Name == "All" && m.GetParameters().Length == 2);

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
            if (filter?.Expressions == null || !filter.Expressions.Any())
            {
                return null;
            }

            var parameter = Expression.Parameter(typeof(TEntity), "entity");
            var allExpressions = BuildExpression(parameter, filter);

            return Expression.Lambda<Func<TEntity, bool>>(allExpressions, parameter);
        }

        private static Expression BuildExpression(ParameterExpression parameter, Filter filter)
        {
            // json support

            object ValueForMember(object val, MemberExpression memberExpression)
            {
                var memberType = memberExpression.Type;
                return FormatValue(val, memberType);
            }

            object FormatArray(object val, MemberExpression memberExpression)
            {
                if (!(val is JToken))
                {
                    return val;
                }

                var array = ((JToken)val).ToObject<object[]>();
                var type = memberExpression.Type;
                return (from object o in array select o.GetType() != type ? Convert.ChangeType(o, type) : type)
                    .ToList();
            }

            Expression allExpressions = null;
            foreach (var filterExpression in filter.Expressions)
            {
                var property = GetMemberExpression(parameter, filterExpression.Property);
                Expression currentExpression = null;
                switch (filterExpression.ExpressionType)
                {
                    case FilterExpressionTypes.Contains:
                        var value = FormatValue(filterExpression.Value, typeof(string)) as string;
                        if (value == null) break;
                        var convertedProperty =
                            Expression.Call(Expression.Call(property, ToStringMethod), ToLowerMethod);
                        currentExpression = Expression.AndAlso(
                            Expression.NotEqual(property, Expression.Constant(null, property.Type)),
                            Expression.Call(convertedProperty, StringContainsMethod,
                                Expression.Constant(value.ToLower())));
                        break;
                    case FilterExpressionTypes.Equal:
                        currentExpression = Expression.Equal(property,
                            Expression.Constant(ValueForMember(filterExpression.Value, property), property.Type));
                        break;
                    case FilterExpressionTypes.MoreThan:
                        currentExpression =
                            Expression.GreaterThan(property,
                                Expression.Constant(ValueForMember(filterExpression.Value, property), property.Type));
                        break;
                    case FilterExpressionTypes.LessThan:
                        currentExpression =
                            Expression.LessThan(property,
                                Expression.Constant(ValueForMember(filterExpression.Value, property), property.Type));
                        break;
                    case FilterExpressionTypes.MoreThanOrEqual:
                        currentExpression =
                            Expression.GreaterThanOrEqual(property,
                                Expression.Constant(ValueForMember(filterExpression.Value, property), property.Type));
                        break;
                    case FilterExpressionTypes.LessThanOrEqual:
                        currentExpression =
                            Expression.LessThanOrEqual(property,
                                Expression.Constant(ValueForMember(filterExpression.Value, property), property.Type));
                        break;
                    case FilterExpressionTypes.In:
                        var inputArray = (ICollection)FormatArray(filterExpression.Value, property);
                        var items = (from object item
                                    in inputArray
                                select Expression
                                    .Constant(item, property.Type))
                            .Cast<Expression>()
                            .ToList();
                        var itemType = items.First().Type;
                        var arrayExpression = Expression.NewArrayInit(itemType, items);
                        var containsMethod = typeof(ICollection<>).MakeGenericType(itemType).GetMethod("Contains");
                        currentExpression =
                            Expression.Call(arrayExpression,
                                containsMethod ?? throw new InvalidOperationException(
                                    "Contains method is not found for 'In' operation. Please check your request.")
                                , property);
                        break;
                    case FilterExpressionTypes.Filter:
                        currentExpression = BuildExpression(parameter,
                            (Filter)FormatValue(filterExpression.Value, typeof(Filter)));
                        break;
                    case FilterExpressionTypes.NotEqual:
                        currentExpression =
                            Expression.NotEqual(property,
                                Expression.Constant(ValueForMember(filterExpression.Value, property), property.Type));
                        break;
                    case FilterExpressionTypes.EqualToDate:
                        currentExpression = Expression.Equal(Expression.Property(property, "Date"),
                            Expression.Constant(Convert.ToDateTime(filterExpression.Value).Date));
                        break;
                    case FilterExpressionTypes.Any:
                        if (!IsItACollection(property))
                        {
                            throw new NextApiException(NextApiErrorCode.UnsupportedFilterOperation,
                                "Filter expression 'Any' supported only for generic 'Enumerable'-like properties");
                        }

                        var collectionAnyItemType = property.Type.GetGenericArguments().First();
                        // process as any without predicate
                        if (filterExpression.Value == null)
                        {
                            var genericAnyMethod = Any.MakeGenericMethod(collectionAnyItemType);
                            currentExpression = Expression.Call(genericAnyMethod, property);
                        }
                        // in case we have predicate, call another any implementation
                        else
                        {
                            var genericAnyMethod = AnyWithPredicate.MakeGenericMethod(collectionAnyItemType);
                            var anyFilterPredicate = MakeNestedFilter(collectionAnyItemType, filterExpression);
                            currentExpression = Expression.Call(genericAnyMethod, property, anyFilterPredicate);
                        }

                        break;
                    case FilterExpressionTypes.All:
                        if (!IsItACollection(property))
                        {
                            throw new NextApiException(NextApiErrorCode.UnsupportedFilterOperation,
                                "Filter expression 'All' supported only for generic 'Enumerable'-like properties");
                        }

                        var collectionAllItemType = property.Type.GetGenericArguments().First();
                        var genericAllMethod = All.MakeGenericMethod(collectionAllItemType);
                        var allFilterPredicate = MakeNestedFilter(collectionAllItemType, filterExpression);
                        currentExpression = Expression.Call(genericAllMethod, property, allFilterPredicate);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                Expression tempExpression;
                if (allExpressions != null)
                {
                    tempExpression = filter.LogicalOperator switch
                    {
                        LogicalOperators.Or => Expression.OrElse(allExpressions, currentExpression),
                        LogicalOperators.Not => Expression.AndAlso(allExpressions, Expression.Not(currentExpression)),
                        LogicalOperators.And => Expression.AndAlso(allExpressions, currentExpression),
                        _ => throw new ArgumentOutOfRangeException()
                    };
                }
                else
                {
                    tempExpression = filter.LogicalOperator == LogicalOperators.Not
                        ? Expression.Not(currentExpression)
                        : currentExpression;
                }

                if (currentExpression != null)
                    allExpressions = tempExpression;
            }

            return allExpressions;
        }

        private static object FormatValue(object val, Type type) => val is JToken token ? token.ToObject(type) : val;

        private static bool IsItACollection(Expression property) =>
            typeof(IEnumerable).IsAssignableFrom(property.Type) && property.Type.IsGenericType;

        private static LambdaExpression MakeNestedFilter(Type collectionItemType, FilterExpression filterExpression)
        {
            var collectionItemParameter = Expression.Parameter(collectionItemType);
            var compiledFilter = BuildExpression(collectionItemParameter,
                (Filter)FormatValue(filterExpression.Value, typeof(Filter)));
            var predicateType = typeof(Func<,>).MakeGenericType(collectionItemType, typeof(bool));
            var filterPredicate =
                Expression.Lambda(predicateType, compiledFilter, collectionItemParameter);
            return filterPredicate;
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
