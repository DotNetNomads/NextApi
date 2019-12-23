using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Abitech.NextApi.Testing.Security.Auth
{
    /// <summary>
    /// Fake-auth extensions
    /// </summary>
    public static class TestAuthExtensions
    {
        /// <summary>
        /// Add fake auth services
        /// </summary>
        /// <param name="services"></param>
        public static void AddFakeAuthServices(this IServiceCollection services)
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
