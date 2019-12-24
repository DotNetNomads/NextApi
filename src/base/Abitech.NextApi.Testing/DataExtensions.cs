using System;
using Abitech.NextApi.Server.EfCore.DAL;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Abitech.NextApi.Testing
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
    }
}
