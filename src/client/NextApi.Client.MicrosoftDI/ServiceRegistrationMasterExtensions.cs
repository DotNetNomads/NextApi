using System;
using Microsoft.Extensions.DependencyInjection;

namespace NextApi.Client.MicrosoftDI
{
    /// <summary>
    /// Extensions for ServiceRegistrationMaster integration with MicrosoftDI
    /// </summary>
    public static class ServiceRegistrationMasterExtensions
    {
        /// <summary>
        /// Register all services in ServiceCollection
        /// </summary>
        /// <param name="registrationMaster">Current service registration manager instance</param>
        /// <param name="services">IServiceCollection instance</param>
        /// <param name="clientFactory">Function used as client factory</param>
        /// <typeparam name="TTokenProvider">Type of access token provider</typeparam>
        /// <typeparam name="TClient">Type of client</typeparam>
        public static void RegisterInMicrosoftDI<TTokenProvider, TClient>(
            this ServiceRegistrationMaster registrationMaster, IServiceCollection services,
            Func<TTokenProvider, TClient> clientFactory)
            where TTokenProvider : class, INextApiAccessTokenProvider
            where TClient : class, INextApiClient
        {
            services.AddSingleton<TClient>(c =>
            {
                var provider = c.GetService<TTokenProvider>();
                return clientFactory(provider);
            });
            // services
            registrationMaster.ManualRegistration(serviceInfo =>
            {
                services.AddTransient(serviceInfo.InterfaceType, serviceInfo.ImplementationType);
            });
        }
    }
}
