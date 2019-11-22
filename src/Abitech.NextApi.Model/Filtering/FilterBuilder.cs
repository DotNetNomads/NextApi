using System;

namespace Abitech.NextApi.Model.Filtering
{
    /// <summary>
    /// Helps with filter building
    /// </summary>
    public class FilterBuilder
    {
        private readonly Filter _filter;


        /// <summary>
        /// Initializes filter builder
        /// </summary>
        /// <param name="logicalOperator">Logical operation for expressions (default: and)</param>
        public FilterBuilder(LogicalOperators logicalOperator = LogicalOperators.And)
        {
            _filter = new Filter {LogicalOperator = logicalOperator};
        }

        /// <summary>
        /// Operation represents: string.Contains()
        /// </summary>
        /// <param name="property">Name of property</param>
        /// <param name="value"></param>
        /// <returns>Current filter builder instance (for chaining)</returns>
        /// <remarks>Supported for all types convertible to string</remarks>
        public FilterBuilder Contains(string property, string value)
        {
            AddExpression(property, FilterExpressionTypes.Contains, value);
            return this;
        }

        /// <summary>
        /// Operation represents: property == value
        /// </summary>
        /// <param name="property"></param>
        /// <param name="value"></param>
        /// <typeparam name="TProperty">Type of property</typeparam>
        /// <returns>Current filter builder instance (for chaining)</returns>
        public FilterBuilder Equal<TProperty>(string property, TProperty value)
        {
            AddExpression(property, FilterExpressionTypes.Equal, value);
            return this;
        }

        /// <summary>
        /// Operation represents: property == value for dates
        /// </summary>
        /// <param name="property"></param>
        /// <param name="value"></param>
        /// <typeparam name="TProperty">Type of property</typeparam>
        /// <returns>Current filter builder instance (for chaining)</returns>
        public FilterBuilder EqualToDate<TProperty>(string property, TProperty value)
        {
            AddExpression(property, FilterExpressionTypes.EqualToDate, value);
            return this;
        }

        /// <summary>
        /// Operation represents: property != value
        /// </summary>
        /// <param name="property"></param>
        /// <param name="value"></param>
        /// <typeparam name="TProperty">Type of property</typeparam>
        /// <returns>Current filter builder instance (for chaining)</returns>
        public FilterBuilder NotEqual<TProperty>(string property, TProperty value)
        {
            AddExpression(property, FilterExpressionTypes.NotEqual, value);
            return this;
        }

        /// <summary>
        /// Checks that property is null
        /// </summary>
        /// <param name="property"></param>
        /// <returns>Current filter builder instance (for chaining)</returns>
        public FilterBuilder Null(string property)
        {
            return Equal<object>(property, null);
        }

        /// <summary>
        /// Checks that property is not null
        /// </summary>
        /// <param name="property"></param>
        /// <returns>Current filter builder instance (for chaining)</returns>
        public FilterBuilder NotNull(string property)
        {
            return NotEqual<object>(property, null);
        }

        /// <summary>
        /// Operation represents: property &gt; value
        /// </summary>
        /// <param name="property"></param>
        /// <param name="value"></param>
        /// <typeparam name="TProperty">Type of property</typeparam>
        /// <returns>Current filter builder instance (for chaining)</returns>
        public FilterBuilder MoreThan<TProperty>(string property, TProperty value)
        {
            AddExpression(property, FilterExpressionTypes.MoreThan, value);
            return this;
        }

        /// <summary>
        /// Operation represents: property &lt; value
        /// </summary>
        /// <param name="property"></param>
        /// <param name="value"></param>
        /// <typeparam name="TProperty">Type of property</typeparam>
        /// <returns>Current filter builder instance (for chaining)</returns>
        public FilterBuilder LessThan<TProperty>(string property, TProperty value)
        {
            AddExpression(property, FilterExpressionTypes.LessThan, value);
            return this;
        }

        /// <summary>
        /// Operation represents: property &gt;= value
        /// </summary>
        /// <param name="property"></param>
        /// <param name="value"></param>
        /// <typeparam name="TProperty">Type of property</typeparam>
        /// <returns>Current filter builder instance (for chaining)</returns>
        public FilterBuilder MoreThanOrEqual<TProperty>(string property, TProperty value)
        {
            AddExpression(property, FilterExpressionTypes.MoreThanOrEqual, value);
            return this;
        }

        /// <summary>
        /// Operation represents: property &lt;= value
        /// </summary>
        /// <param name="property"></param>
        /// <param name="value"></param>
        /// <typeparam name="TProperty">Type of property</typeparam>
        /// <returns>Current filter builder instance (for chaining)</returns>
        public FilterBuilder LessThanOrEqual<TProperty>(string property, TProperty value)
        {
            AddExpression(property, FilterExpressionTypes.LessThanOrEqual, value);
            return this;
        }

        /// <summary>
        /// Operation represents: array.Contains(property)
        /// </summary>
        /// <param name="property"></param>
        /// <param name="array"></param>
        /// <typeparam name="TProperty">Type of property</typeparam>
        /// <returns>Current filter builder instance (for chaining)</returns>
        public FilterBuilder In<TProperty>(string property, TProperty[] array)
        {
            AddExpression(property, FilterExpressionTypes.In, array);
            return this;
        }

        /// <summary>
        /// Operation represents logical expressions group (AND)
        /// </summary>
        /// <param name="builder">Filter builder for current group</param>
        /// <returns>Current filter builder instance (for chaining)</returns>
        public FilterBuilder And(Action<FilterBuilder> builder)
        {
            var filterBuilder = new FilterBuilder(LogicalOperators.And);
            builder.Invoke(filterBuilder);
            AddFilter(filterBuilder.Build());
            return this;
        }

        /// <summary>
        /// Operation represents logical expressions group (AND)
        /// </summary>
        /// <param name="builder">Filter builder for current group</param>
        /// <returns>Current filter builder instance (for chaining)</returns>
        public FilterBuilder And(FilterBuilder builder)
        {
            var filter = builder.Build();
            filter.LogicalOperator = LogicalOperators.And;
            AddFilter(filter);
            return this;
        }

        /// <summary>
        /// Operation represents logical expressions group (OR)
        /// </summary>
        /// <param name="builder">Filter builder for current group</param>
        /// <returns>Current filter builder instance (for chaining)</returns>
        public FilterBuilder Or(Action<FilterBuilder> builder)
        {
            var filterBuilder = new FilterBuilder(LogicalOperators.Or);
            builder.Invoke(filterBuilder);
            AddFilter(filterBuilder.Build());
            return this;
        }

        /// <summary>
        /// Operation represents logical expressions group (OR)
        /// </summary>
        /// <param name="builder">Filter builder for current group</param>
        /// <returns>Current filter builder instance (for chaining)</returns>
        public FilterBuilder Or(FilterBuilder builder)
        {
            var filter = builder.Build();
            filter.LogicalOperator = LogicalOperators.Or;
            AddFilter(filter);
            return this;
        }

        /// <summary>
        /// Operation represents logical expressions group (NOT)
        /// </summary>
        /// <param name="builder">Filter builder for current group</param>
        /// <returns>Current filter builder instance (for chaining)</returns>
        public FilterBuilder Not(Action<FilterBuilder> builder)
        {
            var filterBuilder = new FilterBuilder(LogicalOperators.Not);
            builder.Invoke(filterBuilder);
            AddFilter(filterBuilder.Build());
            return this;
        }

        /// <summary>
        /// Operation represents logical expressions group (NOT)
        /// </summary>
        /// <param name="builder">Filter builder for current group</param>
        /// <returns>Current filter builder instance (for chaining)</returns>
        public FilterBuilder Not(FilterBuilder builder)
        {
            var filter = builder.Build();
            filter.LogicalOperator = LogicalOperators.Not;
            AddFilter(filter);
            return this;
        }

        /// <summary>
        /// Verifies the collection property contains any element.
        /// </summary>
        /// <param name="collectionPropertyName">Collection property name</param>
        /// <returns>Current filter builder instance (for chaining)</returns>
        public FilterBuilder Any(string collectionPropertyName)
        {
            AddExpression(collectionPropertyName, FilterExpressionTypes.Any, null);
            return this;
        }

        /// <summary>
        /// Verifies that collection has one or many elements with accordance for a filter expression.
        /// </summary>
        /// <param name="collectionPropertyName">Collection property name</param>
        /// <param name="builder">Filter builder for current group</param>
        /// <returns>Current filter builder instance (for chaining)</returns>
        public FilterBuilder Any(string collectionPropertyName, Action<FilterBuilder> builder)
        {
            var filterBuilder = new FilterBuilder();
            builder.Invoke(filterBuilder);
            return Any(collectionPropertyName, filterBuilder);
        }

        /// <summary>
        /// Verifies that collection has one or many elements with accordance for a filter expression.
        /// </summary>
        /// <param name="collectionPropertyName">Collection property name</param>
        /// <param name="builder">Filter builder for current group</param>
        /// <returns>Current filter builder instance (for chaining)</returns>
        public FilterBuilder Any(string collectionPropertyName, FilterBuilder builder)
        {
            AddExpression(collectionPropertyName, FilterExpressionTypes.Any, builder.Build());
            return this;
        }

        /// <summary>
        /// Verifies every element of that collection has  accordance for a filter expression.
        /// </summary>
        /// <param name="collectionPropertyName">Collection property name</param>
        /// <param name="builder">Filter builder for current group</param>
        /// <returns>Current filter builder instance (for chaining)</returns>
        public FilterBuilder All(string collectionPropertyName, Action<FilterBuilder> builder)
        {
            var filterBuilder = new FilterBuilder();
            builder.Invoke(filterBuilder);
            return All(collectionPropertyName, filterBuilder);
        }

        /// <summary>
        /// Verifies every element of that collection has  accordance for a filter expression.
        /// </summary>
        /// <param name="collectionPropertyName">Collection property name</param>
        /// <param name="builder">Filter builder for current group</param>
        /// <returns>Current filter builder instance (for chaining)</returns>
        public FilterBuilder All(string collectionPropertyName, FilterBuilder builder)
        {
            AddExpression(collectionPropertyName, FilterExpressionTypes.All, builder.Build());
            return this;
        }

        /// <summary>
        /// Verifies every element of that collection hasn't  accordance for a filter expression.
        /// </summary>
        /// <param name="collectionPropertyName">Collection property name</param>
        /// <param name="builder">Filter builder for current group</param>
        /// <returns>Current filter builder instance (for chaining)</returns>
        public FilterBuilder AllNot(string collectionPropertyName, Action<FilterBuilder> builder)
        {
            var filterBuilder = new FilterBuilder();
            builder.Invoke(filterBuilder);
            return AllNot(collectionPropertyName, filterBuilder);
        }

        /// <summary>
        /// Verifies every element of that collection hasn't  accordance for a filter expression.
        /// </summary>
        /// <param name="collectionPropertyName">Collection property name</param>
        /// <param name="builder">Filter builder for current group</param>
        /// <returns>Current filter builder instance (for chaining)</returns>
        public FilterBuilder AllNot(string collectionPropertyName, FilterBuilder builder)
        {
            var filterBuilder = new FilterBuilder().Not(builder);
            return All(collectionPropertyName, filterBuilder);
        }

        private void AddFilter(Filter filter)
        {
            _filter.Expressions.Add(
                new FilterExpression {ExpressionType = FilterExpressionTypes.Filter, Value = filter});
        }

        private void AddExpression(string property, FilterExpressionTypes expressionType, object value)
        {
            _filter.Expressions.Add(new FilterExpression
            {
                Property = property, ExpressionType = expressionType, Value = value
            });
        }

        /// <summary>
        /// Builds and returns filter
        /// </summary>
        /// <returns></returns>
        public Filter Build()
        {
            return _filter;
        }
    }
}
