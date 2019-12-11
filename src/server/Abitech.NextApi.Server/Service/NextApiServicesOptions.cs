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

        /// <summary>
        /// Size in bytes for single message (actual for SignalR mode). Default is 256KB 
        /// </summary>
        public long MaximumReceiveMessageSize { get; set; } = 256000;
    }
}
