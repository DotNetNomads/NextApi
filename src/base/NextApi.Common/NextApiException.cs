using System;
using System.Collections.Generic;

namespace NextApi.Common
{
    /// <inheritdoc />
    public class NextApiException : Exception
    {
        /// <summary>
        /// Error code
        /// </summary>
        public string Code { get; }

        /// <summary>
        /// Additional parameters for current error
        /// </summary>
        public Dictionary<string, object> Parameters { get; }

        /// <inheritdoc />
        public NextApiException(string code, string message, Dictionary<string, object> parameters = null) :
            base(message)
        {
            Code = code;
            Parameters = parameters;
        }

        /// <summary>
        /// Initialize NextApiException 
        /// </summary>
        /// <param name="code">Error code</param>
        /// <param name="message">Message</param>
        /// <param name="parameters">Parameters</param>
        public NextApiException(NextApiErrorCode code, string message, Dictionary<string, object> parameters = null) :
            base(message)
        {
            Code = code.ToString();
            Parameters = parameters;
        }
    }
}
