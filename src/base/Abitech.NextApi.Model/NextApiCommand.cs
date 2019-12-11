using System.Collections.Generic;

namespace Abitech.NextApi.Model
{
    /// <summary>
    /// Represents model received from NextApi clients for resolve and invoke a service
    /// </summary>
    public class NextApiCommand
    {
        /// <summary>
        /// Name of required service
        /// </summary>
        public string Service { get; set; }
        /// <summary>
        /// Method of service
        /// </summary>
        public string Method { get; set; }
        /// <summary>
        /// Method arguments. Will be parsed and passed when method invoke.
        /// </summary>
        public INextApiArgument[] Args { get; set; }
    }
}
