using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Abitech.NextApi.Server.Service
{
    /// <summary>
    /// Provides list of all registered nextapi services
    /// </summary>
    public class NextApiServiceRegistry
    {
        /// <inheritdoc />
        public NextApiServiceRegistry(IDictionary<string, Type> registeredServices)
        {
            _registeredServices = registeredServices;
        }

        private readonly IDictionary<string, Type> _registeredServices;

        /// <summary>
        /// Resolves service type by name
        /// </summary>
        /// <param name="name">Name of service</param>
        /// <returns>Type of service</returns>
        /// <exception cref="ArgumentNullException">When name is null or empty</exception>
        public Type ResolveNextApiServiceType(string name)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));
            _registeredServices.TryGetValue($"{name}Service", out var serviceType);
            return serviceType;
        }
    }
}
