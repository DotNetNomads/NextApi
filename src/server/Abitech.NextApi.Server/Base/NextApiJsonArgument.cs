using Abitech.NextApi.Common;
using Newtonsoft.Json.Linq;

namespace Abitech.NextApi.Server.Base
{
    /// <summary>
    /// Wrapper for regular NextApiArgument usable in JSON conversion
    /// </summary>
    public class NextApiJsonArgument : INamedNextApiArgument
    {

        /// <summary>
        /// Argument value
        /// </summary>
        public JToken Value { get; set; }

        /// <inheritdoc />
        public string Name { get; set; }
    }
}
