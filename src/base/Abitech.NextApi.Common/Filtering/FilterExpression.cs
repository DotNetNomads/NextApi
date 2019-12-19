namespace Abitech.NextApi.Common.Filtering
{
    /// <summary>
    /// Contains definition of filter expression
    /// </summary>
    public class FilterExpression
    {
        /// <summary>
        /// Type of an expression
        /// </summary>
        public FilterExpressionTypes ExpressionType { get; set; }
        /// <summary>
        /// Name of property
        /// </summary>
        public string Property { get; set; }
        /// <summary>
        /// Filter value
        /// </summary>
        public object Value { get; set; }
    }
}
