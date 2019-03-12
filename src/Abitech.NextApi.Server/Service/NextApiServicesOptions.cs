namespace Abitech.NextApi.Server.Service
{
    /// <summary>
    /// Main options for NextApiServices
    /// </summary>
    public class NextApiServicesOptions
    {
        /// <summary>
        /// Allow services called by anonymous user by default
        /// </summary>
        public bool AnonymousByDefault { get; set; } = false;
        /// <summary>
        /// Disable permission validation for all services
        /// </summary>
        public bool DisablePermissionValidation { get; set; } = false;
    }
}
