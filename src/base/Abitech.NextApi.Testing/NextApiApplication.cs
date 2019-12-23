using System;
using System.Collections.Generic;
using System.Linq;
using Abitech.NextApi.Client;
using Abitech.NextApi.Common.Abstractions;
using Abitech.NextApi.Server.Tests;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Abitech.NextApi.Testing
{
    /// <summary>
    /// Base class for NextApi Application
    /// </summary>
    /// <typeparam name="TServerStartup">Type of server's Startup class</typeparam>
    /// <typeparam name="TClient">Type of NextApi client</typeparam>
    public abstract class NextApiApplication<TServerStartup, TClient> :
        WebApplicationFactory<TServerStartup>, INextApiApplication<TClient>
        where TServerStartup : class, new()
        where TClient : class, INextApiClient
    {
        /// <summary>
        /// Set loging level
        /// </summary>
        protected LogLevel LogLevel = LogLevel.Error;

        private readonly IEnumerable<(Type interfaceType, Type implementationType)> _servicesInfo;

        /// <inheritdoc />
        protected override IWebHostBuilder CreateWebHostBuilder() =>
            WebHost.CreateDefaultBuilder()
                .UseStartup<TServerStartup>()
                .ConfigureLogging(l => l.SetMinimumLevel(LogLevel));

        /// <inheritdoc />
        protected override TestServer CreateServer(IWebHostBuilder builder)
        {
            builder.UseContentRoot(".");
            return base.CreateServer(builder);
        }

        /// <summary>
        /// Get all registered NextApi client-side services
        /// </summary>
        /// <returns>ServiceRegistrationMaster</returns>
        protected abstract ServiceRegistrationMaster GetClientServiceRegistry();

        /// <summary>
        /// Build client
        /// </summary>
        /// <param name="tokenProvider">Token provider instance for client</param>
        /// <param name="transport"></param>
        /// <returns>NextApi client instance</returns>
        protected abstract TClient ClientBuilder(TestTokenProvider tokenProvider, NextApiTransport transport);

        /// <summary>
        /// Do additional registration at client-side DI engine
        /// </summary>
        /// <param name="collection">ServiceCollection instance</param>
        protected virtual void AdditionalClientRegistrations(IServiceCollection collection) { }


        /// <inheritdoc />
        public TClient ResolveClient(string token = null, NextApiTransport transport = NextApiTransport.SignalR)
        {
            var client = ClientBuilder(string.IsNullOrEmpty(token) ? null : new TestTokenProvider(token), transport);
            client.MessageHandler = Server.CreateHandler();
            return client;
        }

        private IServiceCollection ResolveServiceCollection()
        {
            var serviceCollection = new ServiceCollection();
            AdditionalClientRegistrations(serviceCollection);
            return serviceCollection;
        }

        /// <inheritdoc />
        public TService ResolveService<TService>(string token = null,
            NextApiTransport transport = NextApiTransport.SignalR) where TService : INextApiService
        {
            var type = typeof(TService);
            var implementationInfo = _servicesInfo.FirstOrDefault(s => s.interfaceType == type);
            if (implementationInfo == (null, null))
                throw new InvalidOperationException($"Service {type.Name} is not registered!");
            var client = ResolveClient(token, transport);
            var serviceCollection = ResolveServiceCollection();
            serviceCollection.AddTransient(provider => client);
            serviceCollection.AddTransient(implementationInfo.interfaceType, implementationInfo.implementationType);
            return serviceCollection.BuildServiceProvider().GetService<TService>();
        }

        /// <inheritdoc />
        public IServiceProvider ServerServices => Server.Services;

        /// <inheritdoc />
        protected NextApiApplication()
        {
            var servicesInfo = new List<(Type interfaceType, Type implementationType)>();
            GetClientServiceRegistry()
                .ManualRegistration(s => servicesInfo.Add((s.InterfaceType, s.ImplementationType)));
            _servicesInfo = servicesInfo;
        }
    }
}
