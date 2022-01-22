namespace NextApi.Common.Entity
{
    /// <summary>
    /// Interface indicates that entity can be soft deleted
    /// </summary>
    public interface ISoftDeletableEntity
    {
        /// <summary>
        /// Indicates that entity soft-deleted or not
        /// </summary>
        bool IsRemoved { get; set; }
    }
}
