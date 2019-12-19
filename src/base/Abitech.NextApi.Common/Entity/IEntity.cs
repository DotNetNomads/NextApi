namespace Abitech.NextApi.Common.Entity
{
    /// <summary>
    /// Base interface for entity
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    public interface IEntity<TKey>
    {
        /// <summary>
        /// Db identifier
        /// </summary>
        TKey Id { get; set; }
    }
}
