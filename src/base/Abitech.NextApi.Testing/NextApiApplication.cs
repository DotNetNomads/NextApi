using System;
using System.Collections.Generic;
using System.Linq;
using Abitech.NextApi.Client;
using Abitech.NextApi.Common.Abstractions;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Abitech.NextApi.Server.Tests
{
    public abstract class NextApiApplication<TServerStartup, TClient> :
        WebApplicationFactory<TServerStartup>, INextApiApplication<TClient>
        where TServerStartup : class, new()
        where TClient : class, INextApiClient
    {
        protected LogLevel LogLevel = LogLevel.Error;
        private readonly IEnumerable<(Type interfaceType, Type implementationType)> _servicesInfo;

        protected override IWebHostBuilder CreateWebHostBuilder() =>
            WebHost.CreateDefaultBuilder()
                .UseStartup<TServerStartup>()
                .ConfigureLogging(l => l.SetMinimumLevel(LogLevel));

        protected override TestServer CreateServer(IWebHostBuilder builder)
        {
            builder.UseContentRoot(".");
            return base.CreateServer(builder);
        }

        protected abstract ServiceRegistrationMaster GetClientServiceRegistry();
        protected abstract TClient ClientBuilder(TestTokenProvider tokenProvider);
        protected virtual void AdditionalClientRegistrations(IServiceCollection collection) { }


        public TClient ResolveClient(string token = null)
        {
            var client = ClientBuilder(string.IsNullOrEmpty(token) ? null : new TestTokenProvider(token));
            client.MessageHandler = Server.CreateHandler();
            return client;
        }

        private IServiceCollection ResolveServiceCollection()
        {
            var serviceCollection = new ServiceCollection();
            AdditionalClientRegistrations(serviceCollection);
            return serviceCollection;
        }

        public TService ResolveService<TService>(string token = null) where TService : INextApiService
        {
            var type = typeof(TService);
            var implementationInfo = _servicesInfo.FirstOrDefault(s => s.interfaceType == type);
            if (implementationInfo == (null, null))
                throw new InvalidOperationException($"Service {type.Name} is not registered!");
            var client = ResolveClient(token);
            var serviceCollection = ResolveServiceCollection();
            serviceCollection.AddTransient(provider => client);
            serviceCollection.AddTransient(implementationInfo.interfaceType, implementationInfo.implementationType);
            return serviceCollection.BuildServiceProvider().GetService<TService>();
        }

        public IServiceProvider ServerServices => Server.Services;

        protected NextApiApplication()
        {
            var servicesInfo = new List<(Type interfaceType, Type implementationType)>();
            GetClientServiceRegistry()
                .ManualRegistration(s => servicesInfo.Add((s.InterfaceType, s.ImplementationType)));
            _servicesInfo = servicesInfo;
        }
    }
}
