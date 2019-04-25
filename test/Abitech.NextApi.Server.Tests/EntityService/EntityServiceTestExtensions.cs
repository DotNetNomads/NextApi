using Abitech.NextApi.Server.EfCore;
using Abitech.NextApi.Server.Tests.EntityService.DAL;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Abitech.NextApi.Server.Tests.EntityService
{
    public static class EntityServiceTestExtensions
    {
        public static void AddEntityServiceTestsInfrastructure(this IServiceCollection services)
        {
            services.AddDbContext<TestDbContext>(options => options.UseInMemoryDatabase("TestAbitechDb"));
            services.AddTransient<TestUserRepository>();
            services.AddTransient<ITestCityRepository, TestCityRepository>();
            services.AddTransient<TestUnitOfWork>();
            services.AddAutoMapper(typeof(TestDTOProfile));
            services.AddColumnChangesLogger<TestDbContext>();
        }
    }
}
