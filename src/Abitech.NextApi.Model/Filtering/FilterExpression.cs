namespace Abitech.NextApi.Model.Filtering
{
    public class FilterExpression
    {
        public FilterExpressionTypes ExpressionType { get; set; }
        public string Property { get; set; }
        public dynamic Value { get; set; }
    }
}