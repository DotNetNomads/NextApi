using System.Collections.Generic;
using Abitech.NextApi.Server.Service;
using Abitech.NextApi.Server.Tests.EntityService;
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
            services.AddNextApiServices(options =>
            {
                options.DisablePermissionValidation = true;
                options.AnonymousByDefault = true;
            });
            services.AddEntityServiceTestsInfrastructure();
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseNextApiServices();
        }
    }
}
