namespace Abitech.NextApi.Server.EfCore.Model.Base
{
    /// <summary>
    /// Interface represents entity attached to specific Location
    /// </summary>
    public interface ILocationEntity
    {
        /// <summary>
        /// External location identifier (from hr)
        /// </summary>
        int? ExternalLocationId { get; set; }
    }
}
