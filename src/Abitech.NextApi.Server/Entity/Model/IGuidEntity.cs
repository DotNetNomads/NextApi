namespace Abitech.NextApi.Server.Entity.Model
{
    /// <summary>
    /// Base interface for entity with row guid
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    public interface IGuidEntity<TKey> : IEntity<TKey>, IRowGuidEnabled
    {
    }
}
