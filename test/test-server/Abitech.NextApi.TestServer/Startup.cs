using System;
using Abitech.NextApi.Server;
using Abitech.NextApi.Server.EfCore;
using Abitech.NextApi.Server.UploadQueue;
using Abitech.NextApi.Server.UploadQueue.ChangeTracking;
using Abitech.NextApi.Testing;
using Abitech.NextApi.Testing.Data;
using Abitech.NextApi.Testing.Security.Auth;
using Abitech.NextApi.TestServer.DAL;
using Abitech.NextApi.TestServer.Model;
using Abitech.NextApi.TestServer.UploadQueueHandlers;
using AutoMapper;
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
            services.AddFakeDbContext<ITestDbContext, TestDbContext>();
            services.AddDefaultUnitOfWork();
            services.AddTransient<IUploadQueueChangesHandler<TestCity>, TestUploadQueueChangesHandler>();
            services.AddCustomRepo<TestUser, int, ITestUserRepository, TestUserRepository>();
            services.AddDefaultRepo<TestTreeItem, int>();
            services.AddDefaultRepo<TestCity, Guid>();
            services.AddAutoMapper(typeof(TestDTOProfile));
            services.AddColumnChangesLogger<ITestDbContext>();
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseAuthentication();
            app.UseNextApiServices();
        }
    }
}
