using NextApi.Common.Paged;

namespace NextApi.Common.Tree
{
    /// <summary>
    /// Represents query contract for entity tree
    /// </summary>
    public class TreeRequest<TParentKey>
    {
        /// <summary>
        /// Parent identifier
        /// </summary>
        public TParentKey ParentId { get; set; }
        /// <summary>
        /// Expand references aka Include
        /// </summary>
        public PagedRequest PagedRequest { get; set; }
    }
}
