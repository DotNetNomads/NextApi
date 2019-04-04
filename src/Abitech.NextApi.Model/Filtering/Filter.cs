using System.Collections.Generic;

namespace Abitech.NextApi.Model.Filtering
{
    public class Filter
    {
        public LogicalOperators LogicalOperator { get; set; } = LogicalOperators.And;
        public List<FilterExpression> Expressions { get; set; } = new List<FilterExpression>();
    }
}
