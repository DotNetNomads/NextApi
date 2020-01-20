using System;
using Abitech.NextApi.Server.EfCore.DAL;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Pomelo.EntityFrameworkCore.MySql.Storage;

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
        /// <param name="connectionStringAdditional"></param>
        /// <typeparam name="TInterface">DbContext interface</typeparam>
        /// <typeparam name="TImplementation">DbContext implementation</typeparam>
        public static void AddFakeMySqlDbContext<TInterface, TImplementation>(this IServiceCollection services,
            string connectionStringAdditional = "")
            where TInterface : class, INextApiDbContext
            where TImplementation : DbContext, TInterface
        {
            var dbHost = Environment.GetEnvironmentVariable("NEXTAPI_TESTDB_HOST") ?? "ascoa-local";
            var dbPort = Environment.GetEnvironmentVariable("NEXTAPI_TESTDB_PORT") ?? "3399";
            var dbUser = Environment.GetEnvironmentVariable("NEXTAPI_TESTDB_USER") ?? "root";
            var dbPassword = Environment.GetEnvironmentVariable("NEXTAPI_TESTDB_PWD") ?? "root";
            // NOTE: We should switch to MySQL provider due some limitation in InMemory provider (for example: predicate property = null
            // don't works correct in tree type like entities). Also, we use MySQL everywhere.
            var dbName =
                $"TestDb_{Guid.NewGuid()}";
            services.AddDbContext<TImplementation>(options =>
                options.UseMySql($"Server={dbHost};Port={dbPort};User={dbUser};Database={dbName};Password={dbPassword};{connectionStringAdditional}",
                    c => c.CharSet(CharSet.Utf8)));
            services.AddSingleton<ITestApplicationStatesHandler>(c =>
                new TestApplicationStatesHandler<TImplementation>(c, dbHost, dbPort, dbUser, dbPassword, dbName,
                    connectionStringAdditional));
            Func<IServiceProvider, TImplementation> dbResolvler = c =>
            {
                var statesHandler = c.GetService<ITestApplicationStatesHandler>();
                statesHandler.Start();
                var dbContext = c.GetService<TImplementation>();
                return dbContext;
            };
            services.AddScoped<TInterface>(dbResolvler);
            services.AddScoped<INextApiDbContext>(dbResolvler);
        }

        /// <summary>
        /// Resolve DbContext for NextApi application
        /// </summary>
        /// <param name="application"></param>
        /// <typeparam name="TDbContext">Type of DbContext</typeparam>
        /// <returns>DisposableDbContext for specific DbContext (should be used with "using" keyword. cause db context
        /// is scoped by default)</returns>
        public static DisposableDbContext<TDbContext> ResolveDbContext<TDbContext>(
            this INextApiApplication application) where TDbContext : INextApiDbContext
        {
            var scope = application.ServerServices.CreateScope();
            var dbContext = scope.ServiceProvider.GetService<TDbContext>();
            return new DisposableDbContext<TDbContext>(scope, dbContext);
        }
    }
}
