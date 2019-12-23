using Abitech.NextApi.Server;
using Abitech.NextApi.Server.UploadQueue.ChangeTracking;
using Abitech.NextApi.Testing.Security.Auth;
using Abitech.NextApi.TestServer.Model;
using Abitech.NextApi.TestServer.UploadQueueHandlers;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Abitech.NextApi.TestServer
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            // fake auth
            services.AddFakeAuthServices();

            services.AddNextApiServices(options =>
                {
                    options.AnonymousByDefault = true;
                    options.MaximumReceiveMessageSize = 1000000;
                })
                .AddPermissionProvider<TestPermissionProvider>();
            services.AddInMemoryDbAndRepos();
            services.AddTransient<IUploadQueueChangesHandler<TestCity>, TestUploadQueueChangesHandler>();
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseAuthentication();
            app.UseNextApiServices();
        }
    }
}
