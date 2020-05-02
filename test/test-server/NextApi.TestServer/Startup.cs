using System;
using AutoMapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using NextApi.Server;
using NextApi.Server.EfCore;
using NextApi.Server.Service;
using NextApi.Server.UploadQueue;
using NextApi.Server.UploadQueue.ChangeTracking;
using NextApi.Testing.Data;
using NextApi.Testing.Security.Auth;
using NextApi.TestServer.DAL;
using NextApi.TestServer.DTO;
using NextApi.TestServer.Model;
using NextApi.TestServer.Service;
using NextApi.TestServer.UploadQueueHandlers;

namespace NextApi.TestServer
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            // fake auth
            services.AddFakeAuthServices();

            services.AddNextApiServices(options =>
                {
                    options.MaximumReceiveMessageSize = 1000000;
                    options.AddEntityService<TestUserDTO, TestUser, int>().AllowToGuests();
                    options.AddUploadQueueService("NextApi.TestServer")
                        .AllowToGuests();
                    options.AddTreeEntityService<TestTreeItemDto, TestTreeItem, int, int?>();
                    options.AddService<TestService>().AllowToGuests();
                })
                .AddPermissionProvider<TestPermissionProvider>();
            // OldGuids=true i have no idea why, but without this attribute, Guids can't be mapped to binary(16)
            services.AddFakeMySqlDbContext<ITestDbContext, TestDbContext>("OldGuids=true");
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
