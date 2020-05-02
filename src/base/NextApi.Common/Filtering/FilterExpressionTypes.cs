namespace NextApi.Common.Filtering
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
        Any,
        All
#pragma warning restore 1591
    }
}
