using Abitech.NextApi.Common.Paged;

namespace Abitech.NextApi.Common.Tree
{
    /// <summary>
    /// Represents query contract for entity tree
    /// </summary>
    public class TreeRequest
    {
        /// <summary>
        /// Parent identifier
        /// </summary>
        public object ParentId { get; set; }
        /// <summary>
        /// Expand references aka Include
        /// </summary>
        public PagedRequest PagedRequest { get; set; }
    }
}
