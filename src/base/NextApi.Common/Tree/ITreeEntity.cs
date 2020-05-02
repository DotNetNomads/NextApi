using NextApi.Common.Entity;

namespace NextApi.Common.Tree
{
    /// <summary>
    /// Base interface for Tree-type entity (should has ParentId field)
    /// </summary>
    /// <typeparam name="TKey">Key type</typeparam>
    /// <typeparam name="TParentKey">Parent key type</typeparam>
    public interface ITreeEntity<TKey, TParentKey> : IEntity<TKey>
    {
        /// <summary>
        /// Parent entity identifier
        /// </summary>
        TParentKey ParentId { get; set; }
    }
}
