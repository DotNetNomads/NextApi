using System;
using System.Threading.Tasks;
using Autofac;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Xunit.Abstractions;

namespace Abitech.NextApi.Client.DI.Tests
{
    public class TestServiceRegistrationMaster
    {
        [Fact]
        public void Autofac()
        {
            var testUrl = "http://hello-for-autofac";
            var builder = new ContainerBuilder();
            builder.RegisterType<TestTokenProvider>();
            builder.AddTestClientServices<TestTokenProvider>(testUrl);
            var container = builder.Build();

            var testService = container.Resolve<ITestService>();

            Assert.Equal(testUrl, testService.GetUrl());
        }

        [Fact]
        public void ServiceCollection()
        {
            var testUrl = "http://hello-for-service-collection";
            var collection = new ServiceCollection();
            collection.AddTransient<TestTokenProvider>();
            collection.AddTestClientServices<TestTokenProvider>(testUrl);

            var provider = collection.BuildServiceProvider();

            var testService = provider.GetService<ITestService>();

            Assert.Equal(testUrl, testService.GetUrl());
        }
    }

    #region setup 

    static class TestExtensions
    {
        private static ServiceRegistrationMaster allServices =>
            new ServiceRegistrationMaster()
                .Add<ITestService, TestService>();

        public static void AddTestClientServices<TTokenProvider>(this ContainerBuilder builder, string url)
            where TTokenProvider : class, INextApiAccessTokenProvider
        {
            allServices.RegisterInAutofac<TTokenProvider, ITestClient>(builder,
                tokenProvider => new TestClient(url, tokenProvider));
        }

        public static void AddTestClientServices<TTokenProvider>(this IServiceCollection services, string url)
            where TTokenProvider : class, INextApiAccessTokenProvider
        {
            allServices.RegisterInServiceCollection<TTokenProvider, ITestClient>(services,
                provider => new TestClient(url, provider));
        }
    }

    class TestTokenProvider : INextApiAccessTokenProvider
    {
#pragma warning disable 1998
        public async Task<string> ResolveToken()
#pragma warning restore 1998
        {
            return "";
        }
    }

    class TestClient : NextApiClient, ITestClient
    {
        public TestClient(string url, INextApiAccessTokenProvider tokenProvider, NextApiTransport transportType = NextApiTransport.SignalR,
            bool reconnectAutomatically = true,
            int reconnectDelayMs = 5000) : base(url, tokenProvider, transportType, reconnectAutomatically,
            reconnectDelayMs)
        {
        }

        public string GetUrl()
        {
            return Url;
        }
    }

    interface ITestClient : INextApiClient
    {
        string GetUrl();
    }

    interface ITestService
    {
        string GetUrl();
    }

    class TestService : ITestService
    {
        private ITestClient _testClient;

        public TestService(ITestClient testClient)
        {
            _testClient = testClient ?? throw new ArgumentNullException(nameof(testClient));
        }

        public string GetUrl()
        {
            return _testClient.GetUrl();
        }
    }

    #endregion
}
