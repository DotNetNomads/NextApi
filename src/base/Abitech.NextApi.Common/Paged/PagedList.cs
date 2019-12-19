using System.Collections.Generic;

namespace Abitech.NextApi.Common.Paged
{
    /// <summary>
    /// Represents basic abstraction for provide Entity collection
    /// </summary>
    /// <typeparam name="T">Entity type</typeparam>
    public class PagedList<T>
    {
        /// <summary>
        /// Items selected by PagedRequest
        /// </summary>
        public List<T> Items { get; set; } = new List<T>();
        /// <summary>
        /// Total count of items in repository
        /// </summary>
        public int TotalItems { get; set; }
    }
}