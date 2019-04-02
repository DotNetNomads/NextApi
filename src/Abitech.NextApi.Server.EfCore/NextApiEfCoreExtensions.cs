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
        /// Adds INextApiUserInfoProvider implementation to IServiceCollection
        /// </summary>
        /// <param name="services"></param>
        /// <typeparam name="T">Type of implementation</typeparam>
        /// <returns></returns>
        public static IServiceCollection AddUserInfoProvider<T>(this IServiceCollection services)
            where T : class, INextApiUserInfoProvider
        {
            services.AddTransient<INextApiUserInfoProvider, T>();
            return services;
        }

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
