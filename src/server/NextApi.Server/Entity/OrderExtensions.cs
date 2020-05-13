using System;
using System.Linq;
using System.Linq.Expressions;
using NextApi.Common.Ordering;

namespace NextApi.Server.Entity
{
    /// <summary>
    /// Order extensions
    /// </summary>
    public static class OrderExtensions
    {
        public static IQueryable<TEntity> GenerateOrdering<TEntity>(this IQueryable<TEntity> source, Order[] orders)
        {
            foreach (var order in orders)
            {
                switch (order.OrderOperator)
                {
                    case OrderOperators.OrderBy:
                        source = ApplyOrder(source, order.Property, "OrderBy");
                        break;
                    case OrderOperators.ThenBy:
                        source = ApplyOrder(source, order.Property, "ThenBy");
                        break;
                    case OrderOperators.OrderByDescending:
                        source = ApplyOrder(source, order.Property, "OrderByDescending");
                        break;
                    case OrderOperators.ThenByDescending:
                        source = ApplyOrder(source, order.Property, "ThenByDescending");
                        break;
                    default:
                        throw new ArgumentException(nameof(order.OrderOperator));
                }
            }

            return source;
        }
        
        // more info https://stackoverflow.com/questions/41244/dynamic-linq-orderby-on-ienumerablet-iqueryablet
        public static IOrderedQueryable<TEntity> OrderBy<TEntity>(this IQueryable<TEntity> source, string property)
        {
            return ApplyOrder<TEntity>(source, property, "OrderBy");
        }
        
        public static IOrderedQueryable<TEntity> OrderByDescending<TEntity>(
            this IQueryable<TEntity> source, 
            string property)
        {
            return ApplyOrder<TEntity>(source, property, "OrderByDescending");
        }

        public static IOrderedQueryable<TEntity> ThenBy<TEntity>(
            this IOrderedQueryable<TEntity> source, 
            string property)
        {
            return ApplyOrder<TEntity>(source, property, "ThenBy");
        }

        public static IOrderedQueryable<TEntity> ThenByDescending<TEntity>(
            this IOrderedQueryable<TEntity> source, 
            string property)
        {
            return ApplyOrder<TEntity>(source, property, "ThenByDescending");
        }

        static IOrderedQueryable<TEntity> ApplyOrder<TEntity>(
            IQueryable<TEntity> source, 
            string property, 
            string methodName) 
        {
            var props = property.Split('.');
            var type = typeof(TEntity);
            var arg = Expression.Parameter(type, "x");
            Expression expr = arg;
            foreach(string prop in props) {
                // use reflection (not ComponentModel) to mirror LINQ
                var pi = type.GetProperty(prop);
                expr = Expression.Property(expr, pi);
                type = pi.PropertyType;
            }
            var delegateType = typeof(Func<,>).MakeGenericType(typeof(TEntity), type);
            var lambda = Expression.Lambda(delegateType, expr, arg);

            var result = typeof(Queryable).GetMethods().Single(
                    method => method.Name == methodName
                              && method.IsGenericMethodDefinition
                              && method.GetGenericArguments().Length == 2
                              && method.GetParameters().Length == 2)
                .MakeGenericMethod(typeof(TEntity), type)
                .Invoke(null, new object[] {source, lambda});
            return (IOrderedQueryable<TEntity>)result;
        }
    }
}
