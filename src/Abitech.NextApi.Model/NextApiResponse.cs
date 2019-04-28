using System.Collections.Generic;

namespace Abitech.NextApi.Model
{
    /// <summary>
    /// Default response wrapper from NextApi
    /// </summary>
    public class NextApiResponse
    {
        /// <summary>
        /// Indicates that request is successful
        /// </summary>
        public bool Success { get; set; } = true;

        /// <summary>
        /// Contains data of response
        /// </summary>
        public object Data { get; set; }

        /// <summary>
        /// Contains error if Success is false
        /// </summary>
        public NextApiError Error { get; set; }
    }

    /// <summary>
    /// Default error wrapper from NextApi
    /// </summary>
    public class NextApiError
    {
        /// <summary>
        /// Error code. For example: ServiceNotFound
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// Additional parameters for current error
        /// </summary>
        public Dictionary<string, object> Parameters { get; set; }
    }

    /// <summary>
    /// System error code for nextapi
    /// </summary>
    public enum NextApiErrorCode
    {
#pragma warning disable 1591
        // base services
        ServiceIsNotFound,
        OperationIsNotFound,
        ServiceIsOnlyForAuthorized,
        OperationIsNotAllowed,
        Unknown,
        IncorrectRequest
#pragma warning restore 1591
    }
}
