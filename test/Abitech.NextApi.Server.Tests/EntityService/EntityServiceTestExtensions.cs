using System;
using Abitech.NextApi.Server.EfCore;
using Abitech.NextApi.Server.Tests.EntityService.DAL;
using Abitech.NextApi.Server.UploadQueue;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Abitech.NextApi.Server.Tests.EntityService
{
    public static class EntityServiceTestExtensions
    {
        public static void AddEntityServiceTestsInfrastructure(this IServiceCollection services)
        {
            var dbName = "TestAbitechDb" + Guid.NewGuid();
//            Console.WriteLine($"Db Name: {dbName}");
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
