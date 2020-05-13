using NextApi.Common.Filtering;
using NextApi.Common.Ordering;

namespace NextApi.Common.Paged
{
    /// <summary>
    /// Represents query contract for entity collection
    /// </summary>
    public class PagedRequest
    {
        /// <summary>
        /// Pagination - skip items count
        /// </summary>
        public int? Skip { get; set; }
        /// <summary>
        /// Pagination - take items count
        /// </summary>
        public int? Take { get; set; }
        /// <summary>
        /// Expand references aka Include
        /// </summary>
        public string[] Expand { get; set; }
        
        /// <summary>
        /// Filter instance for this request
        /// </summary>
        public Filter Filter { get; set; }
        
        /// <summary>
        /// Order instances for this request
        /// </summary>
        public Order[] Orders { get; set; }
    }
}
