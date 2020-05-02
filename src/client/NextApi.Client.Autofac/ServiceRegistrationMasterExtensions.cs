using System;
using Autofac;

namespace NextApi.Client.Autofac
{
    /// <summary>
    /// Extensions for ServiceRegistrationMaster integration with Autofac
    /// </summary>
    public static class ServiceRegistrationMasterExtensions
    {
        /// <summary>
        /// Register all services in Autofac
        /// </summary>
        /// <param name="registrationMaster">Current service registration manager instance</param>
        /// <param name="builder">ContainerBuilder instance</param>
        /// <param name="clientFactory">Function used as client factory</param>
        /// <typeparam name="TTokenProvider">Type of access token provider</typeparam>
        /// <typeparam name="TClient">Type of client</typeparam>
        public static void RegisterInAutofac<TTokenProvider, TClient>(this ServiceRegistrationMaster registrationMaster,
            ContainerBuilder builder,
            Func<TTokenProvider, TClient> clientFactory)
            where TTokenProvider : class, INextApiAccessTokenProvider
            where TClient : class, INextApiClient
        {
            builder.Register(c =>
            {
                var provider = c.Resolve<TTokenProvider>();
                return clientFactory(provider);
            }).As<TClient>().SingleInstance();
            // services
            registrationMaster.ManualRegistration(serviceInfo =>
            {
                builder.RegisterType(serviceInfo.ImplementationType).As(serviceInfo.InterfaceType);
            });
        }
    }
}
