using System;
using System.Collections.Generic;
using Autofac;
using Microsoft.Extensions.DependencyInjection;

namespace Abitech.NextApi.Client.DI
{
    /// <summary>
    /// Helps to register services in IoC container
    /// </summary>
    public class ServiceRegistrationMaster
    {
        private readonly Dictionary<Type, Type> _registeredServices = new Dictionary<Type, Type>();

        /// <summary>
        /// Add service to register
        /// </summary>
        /// <typeparam name="TInterface"></typeparam>
        /// <typeparam name="TImplementation"></typeparam>
        /// <returns></returns>
        public ServiceRegistrationMaster Add<TInterface, TImplementation>()
            where TImplementation : class, TInterface
        {
            _registeredServices.Add(typeof(TInterface), typeof(TImplementation));
            return this;
        }

        /// <summary>
        /// Register all services in Autofac
        /// </summary>
        /// <param name="builder">ContainerBuilder instance</param>
        /// <param name="clientFactory">Function used as client factory</param>
        /// <typeparam name="TTokenProvider">Type of access token provider</typeparam>
        /// <typeparam name="TClient">Type of client</typeparam>
        public void RegisterInAutofac<TTokenProvider, TClient>(ContainerBuilder builder,
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
            foreach (var service in _registeredServices)
            {
                builder.RegisterType(service.Value).As(service.Key);
            }
        }

        /// <summary>
        /// Register all services in ServiceCollection
        /// </summary>
        /// <param name="services">IServiceCollection instance</param>
        /// <param name="clientFactory">Function used as client factory</param>
        /// <typeparam name="TTokenProvider">Type of access token provider</typeparam>
        /// <typeparam name="TClient">Type of client</typeparam>
        public void RegisterInServiceCollection<TTokenProvider, TClient>(IServiceCollection services,
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
            foreach (var service in _registeredServices)
            {
                services.AddTransient(service.Key, service.Value);
            }
        }
    }
}
