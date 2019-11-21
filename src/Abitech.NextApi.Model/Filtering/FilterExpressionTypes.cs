namespace Abitech.NextApi.Model.Filtering
{
    /// <summary>
    /// Supported filter expression types
    /// </summary>
    public enum FilterExpressionTypes
    {
#pragma warning disable 1591
        Contains,
        Equal,
        MoreThan,
        LessThan,
        MoreThanOrEqual,
        LessThanOrEqual,
        In,
        Filter,
        NotEqual,
        EqualToDate,
        Any
#pragma warning restore 1591
    }
}
