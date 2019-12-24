using System;
using System.Threading.Tasks;
using Abitech.NextApi.Server.EfCore.DAL;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Abitech.NextApi.Testing.Data
{
    /// <summary>
    /// Extensions for DbContext's
    /// </summary>
    public static class DataExtensions
    {
        /// <summary>
        /// Add fake DbContext to services
        /// </summary>
        /// <param name="services">Service collection instance</param>
        /// <typeparam name="TInterface">DbContext interface</typeparam>
        /// <typeparam name="TImplementation">DbContext implementation</typeparam>
        public static void AddFakeDbContext<TInterface, TImplementation>(this IServiceCollection services)
            where TInterface : INextApiDbContext
            where TImplementation : DbContext, TInterface
        {
            var dbName = "TestNextApiDb" + Guid.NewGuid();
            services.AddDbContext<TInterface, TImplementation>(options =>
                options.UseInMemoryDatabase(dbName));
        }

        /// <summary>
        /// Resolve DbContext for NextApi application
        /// </summary>
        /// <param name="application"></param>
        /// <typeparam name="TDbContext">Type of DbContext</typeparam>
        /// <returns>DisposableDbContext for specific DbContext (should be used with "using" keyword. cause db context
        /// is scoped by default)</returns>
        public static async Task<DisposableDbContext<TDbContext>> ResolveDbContext<TDbContext>(
            this INextApiApplication application) where TDbContext : INextApiDbContext
        {
            var scope = application.ServerServices.CreateScope();
            var dbContext = scope.ServiceProvider.GetService<TDbContext>();
            await (dbContext as DbContext).Database.EnsureCreatedAsync();
            return new DisposableDbContext<TDbContext>(scope, dbContext);
        }
    }
}
