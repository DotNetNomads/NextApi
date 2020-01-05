using Abitech.NextApi.Common.Entity;

namespace Abitech.NextApi.Common.Tree
{
    /// <summary>
    /// Base interface for Tree-type entity (should has ParentId field)
    /// </summary>
    /// <typeparam name="TKey">Key type (only struct-like types)</typeparam>
    public interface ITreeEntity<TKey> : IEntity<TKey>
    {
    }
}
