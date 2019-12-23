using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Tasks.Server.Tests.Common.Auth;

namespace Abitech.NextApi.Server.Tests.Security.Auth
{
    public static class TestAuthExtensions
    {
        public static void AddTestAuthServices(this IServiceCollection services)
        {
            services.AddMvcCore();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = "Tests";
                    options.DefaultChallengeScheme = "Tests";
                })
                .AddScheme<TestAuthOptions, TestAuthHandler>("Tests", "Tests", options => { });
        }
    }
}