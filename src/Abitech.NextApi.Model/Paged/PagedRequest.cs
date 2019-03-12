namespace Abitech.NextApi.Model.Paged
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
    }
}
