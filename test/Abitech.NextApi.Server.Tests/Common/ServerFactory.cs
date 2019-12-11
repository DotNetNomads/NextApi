using System;
using Abitech.NextApi.Server.Tests.EntityService;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Abitech.NextApi.Server.Tests.Common
{
    public class ServerFactory : WebApplicationFactory<TestStartup>
    {
        protected override IWebHostBuilder CreateWebHostBuilder()
        {
            return WebHost.CreateDefaultBuilder()
                .UseStartup<TestStartup>()
                .ConfigureLogging(log => log.SetMinimumLevel(LogLevel.None));
        }

        protected override TestServer CreateServer(IWebHostBuilder builder)
        {
            builder.UseContentRoot(".");
            var server = base.CreateServer(builder);
            return server;
        }
    }
}
