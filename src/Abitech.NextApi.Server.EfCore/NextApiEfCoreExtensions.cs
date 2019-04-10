using Abitech.NextApi.Server.EfCore.DAL;
using Abitech.NextApi.Server.EfCore.Service;
using Microsoft.Extensions.DependencyInjection;

namespace Abitech.NextApi.Server.EfCore
{
    /// <summary>
    /// Useful extensions for IServiceCollection
    /// </summary>
    public static class NextApiEfCoreExtensions
    {

        /// <summary>
        /// Adds ColumnChangesLogger to IServiceCollection
        /// </summary>
        /// <param name="services"></param>
        /// <typeparam name="TDbContext">Db context type</typeparam>
        /// <returns></returns>
        public static IServiceCollection AddColumnChangesLogger<TDbContext>(this IServiceCollection services)
            where TDbContext : class, IColumnChangesEnabledNextApiDbContext
        {
            services.AddScoped<IColumnChangesLogger, ColumnChangesLogger<TDbContext>>();
            return services;
        }
    }
}
