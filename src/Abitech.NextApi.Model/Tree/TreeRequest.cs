namespace Abitech.NextApi.Model.Tree
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
        public string[] Expand { get; set; }
    }
}
