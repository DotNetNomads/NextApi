using System;

namespace Abitech.NextApi.Model.Filtering
{
    public class FilterBuilder
    {
        private Filtering.Filter _filter;

        public FilterBuilder(LogicalOperators logicalOperator = LogicalOperators.And)
        {
            _filter = new Filtering.Filter
            {
                LogicalOperator = logicalOperator
            };
        }

        public FilterBuilder Contains(string property, dynamic value)
        {
            AddExpression(property, FilterExpressionTypes.Contains, value);
            return this;
        }

        public FilterBuilder Equal(string property, dynamic value)
        {
            AddExpression(property, FilterExpressionTypes.Equal, value);
            return this;
        }

        public FilterBuilder NotEqual(string property, dynamic value)
        {
            AddExpression(property, FilterExpressionTypes.NotEqual, value);
            return this;
        }

        public FilterBuilder MoreThan(string property, dynamic value)
        {
            AddExpression(property, FilterExpressionTypes.MoreThan, value);
            return this;
        }

        public FilterBuilder LessThan(string property, dynamic value)
        {
            AddExpression(property, FilterExpressionTypes.LessThan, value);
            return this;
        }

        public FilterBuilder MoreThanOrEqual(string property, dynamic value)
        {
            AddExpression(property, FilterExpressionTypes.MoreThanOrEqual, value);
            return this;
        }

        public FilterBuilder LessThanOrEqual(string property, dynamic value)
        {
            AddExpression(property, FilterExpressionTypes.LessThanOrEqual, value);
            return this;
        }

        public FilterBuilder In(string property, dynamic[] array)
        {
            AddExpression(property, FilterExpressionTypes.In, array);
            return this;
        }

        public FilterBuilder And(Action<FilterBuilder> builder)
        {
            var filterBuilder = new FilterBuilder();
            builder.Invoke(filterBuilder);
            AddFilter(filterBuilder.Build());
            return this;
        }

        public FilterBuilder Or(Action<FilterBuilder> builder)
        {
            var filterBuilder = new FilterBuilder(LogicalOperators.Or);
            builder.Invoke(filterBuilder);
            AddFilter(filterBuilder.Build());
            return this;
        }

        private void AddFilter(Filtering.Filter filter)
        {
            _filter.Expressions.Add(new FilterExpression()
            {
                ExpressionType = FilterExpressionTypes.Filter,
                Value = filter
            });
        }

        private void AddExpression(string property, FilterExpressionTypes expressionType, dynamic value)
        {
            _filter.Expressions.Add(new FilterExpression
            {
                Property = property,
                ExpressionType = expressionType,
                Value = value
            });
        }

        public Filtering.Filter Build()
        {
            return _filter;
        }
    }
}
