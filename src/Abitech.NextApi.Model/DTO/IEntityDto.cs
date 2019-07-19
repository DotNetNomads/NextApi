namespace Abitech.NextApi.Model.DTO
{
    /// <summary>
    /// Basic interface for Entity DTO
    /// </summary>
    public interface IEntityDto<TKey>
    {
        /// <summary>
        /// Entity id
        /// </summary>
        TKey Id { get; set; }
    }
}