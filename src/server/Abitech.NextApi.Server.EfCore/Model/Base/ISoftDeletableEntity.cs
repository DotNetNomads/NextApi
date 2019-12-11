namespace Abitech.NextApi.Server.EfCore.Model.Base
{
    /// <summary>
    /// Interface indicates that entity can be soft deleted
    /// </summary>
    public interface ISoftDeletableEntity
    {
        /// <summary>
        /// Indicates that entity soft-deleted or not
        /// </summary>
        bool IsRemoved { get; set; }
    }
}
