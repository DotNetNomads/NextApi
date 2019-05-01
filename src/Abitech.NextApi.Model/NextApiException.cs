using System;
using System.Collections.Generic;

namespace Abitech.NextApi.Model
{
    /// <inheritdoc />
    public class NextApiException: Exception
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
        public NextApiException(string message, string code, Dictionary<string, object> parameters) : base(message)
        {
            Code = code;
            Parameters = parameters;
        }
    }
}
