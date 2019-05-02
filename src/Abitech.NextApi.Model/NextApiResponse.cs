using System.Collections.Generic;

namespace Abitech.NextApi.Model
{
    /// <summary>
    /// Default response wrapper from NextApi
    /// </summary>
    public class NextApiResponse<TDataType>
    {
        /// <summary>
        /// Indicates that request is successful
        /// </summary>
        public bool Success { get; } = true;

        /// <summary>
        /// Contains data of response
        /// </summary>
        public TDataType Data { get; }

        /// <summary>
        /// Contains error if Success is false
        /// </summary>
        public NextApiError Error { get; }

        /// <inheritdoc />
        public NextApiResponse(TDataType data, NextApiError error = null, bool success = true)
        {
            Data = data;
            Error = error;
            Success = success;
        }
    }

    /// <summary>
    /// Default error wrapper from NextApi
    /// </summary>
    public class NextApiError
    {
        /// <summary>
        /// Error code. For example: ServiceNotFound
        /// </summary>
        public string Code { get; private set; }

        /// <summary>
        /// Additional parameters for current error
        /// </summary>
        public Dictionary<string, object> Parameters { get; private set; }

        /// <inheritdoc />
        public NextApiError(string code, Dictionary<string, object> parameters)
        {
            Code = code;
            Parameters = parameters;
        }
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
        IncorrectRequest,
        HttpError
#pragma warning restore 1591
    }
}
