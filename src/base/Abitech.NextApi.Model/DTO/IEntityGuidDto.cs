namespace Abitech.NextApi.Model.DTO
{
    /// <summary>
    /// Basic interface for Entity DTO wit row guid
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    public interface IEntityGuidDto<TKey> : IEntityDto<TKey>, IRowGuidEnabledDto
    {
        
    }
}