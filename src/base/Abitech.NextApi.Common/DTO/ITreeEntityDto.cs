namespace Abitech.NextApi.Common.DTO
{
    /// <summary>
    /// Basic interface for Tree Entity DTO
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TParentKey"></typeparam>
    public interface ITreeEntityDto<TKey, TParentKey> : IEntityDto<TKey>
    {
        /// <summary>
        /// Parent entity identifier
        /// </summary>
        TParentKey ParentId { get; set; }
    }
}
