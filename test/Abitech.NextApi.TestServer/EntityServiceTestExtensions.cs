using System;
using Abitech.NextApi.Server.UploadQueue;
using Abitech.NextApi.TestServer.DAL;
using Abitech.NextApi.TestServer.Service;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Abitech.NextApi.TestServer
{
    public static class EntityServiceTestExtensions
    {
        public static void AddInMemoryDbAndRepos(this IServiceCollection services)
        {
            var dbName = "TestNextApiDb" + Guid.NewGuid();
            services.AddDbContext<TestDbContext>(options =>
                options.UseInMemoryDatabase(dbName));
            services.AddTransient<TestUserRepository>();
            services.AddTransient<TestTreeItemRepository>();
            services.AddTransient<ITestCityRepository, TestCityRepository>();
            services.AddTransient<TestUnitOfWork>();
            services.AddAutoMapper(typeof(TestDTOProfile));
            services.AddColumnChangesLogger<TestDbContext>();
        }
    }
}
