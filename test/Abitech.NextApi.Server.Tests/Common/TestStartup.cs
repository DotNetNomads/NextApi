using System.Collections.Generic;
using Abitech.NextApi.Common.Abstractions;
using Abitech.NextApi.Server.Service;
using Abitech.NextApi.Server.Tests.EntityService;
using Abitech.NextApi.Server.Tests.EntityService.Model;
using Abitech.NextApi.Server.Tests.Security.Auth;
using Abitech.NextApi.Server.Tests.Service;
using Abitech.NextApi.Server.Tests.System;
using Abitech.NextApi.Server.UploadQueue.ChangeTracking;
using MessagePack;
using MessagePack.Resolvers;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Abitech.NextApi.Server.Tests.Common
{
    public class TestStartup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            // fake auth
            services.AddTestAuthServices();

            services.AddNextApiServices(options =>
                {
                    options.AnonymousByDefault = true;
                    options.MaximumReceiveMessageSize = 1000000;
                })
                .AddPermissionProvider<TestPermissionProvider>();
            services.AddEntityServiceTestsInfrastructure();
            services.AddTransient<IUploadQueueChangesHandler<TestCity>, TestUploadQueueChangesHandler>();
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseTokenQueryToHeaderFormatter();
            app.UseAuthentication();
            app.UseNextApiServices();
        }
    }
}
