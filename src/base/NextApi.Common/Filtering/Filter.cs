using System.Collections.Generic;

namespace NextApi.Common.Filtering
{
    /// <summary>
    /// NextApi Filter
    /// </summary>
    public class Filter
    {
        /// <summary>
        /// Logical operator for this filter
        /// </summary>
        public LogicalOperators LogicalOperator { get; set; } = LogicalOperators.And;
        /// <summary>
        /// Filter expressions
        /// </summary>
        public List<FilterExpression> Expressions { get; set; } = new List<FilterExpression>();
    }
}
