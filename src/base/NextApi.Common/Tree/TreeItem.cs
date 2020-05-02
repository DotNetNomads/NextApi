namespace NextApi.Common.Tree
{
    /// <summary>
    /// Represents entity tree item
    /// </summary>
    public class TreeItem<TEntity> where TEntity : class
    {
        /// <summary>
        /// Entity item
        /// </summary>
        public TEntity Entity { get; set; }
        /// <summary>
        /// Children count for the entity
        /// </summary>
        public int ChildrenCount { get; set; }
    }
}
