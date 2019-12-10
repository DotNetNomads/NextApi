namespace Abitech.NextApi.Server.EfCore.Model.Base
{
    /// <summary>
    /// Interface represents entity attached to specific OrgUnit
    /// </summary>
    public interface IOrgUnitEntity
    {
        /// <summary>
        /// External organization unit identifier (from hr)
        /// </summary>
        int? ExternalOrgUnitId { get; set; }
    }
}
