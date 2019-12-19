using Abitech.NextApi.Server.UploadQueue.ChangeTracking;
using Abitech.NextApi.Server.UploadQueue.DAL;
using Microsoft.Extensions.DependencyInjection;

namespace Abitech.NextApi.Server.UploadQueue
{
    /// <summary>
    /// Useful extensions for IServiceCollection
    /// </summary>
    public static class UploadQueueServerExtensions
    {

        /// <summary>
        /// Adds ColumnChangesLogger to IServiceCollection
        /// </summary>
        /// <param name="services"></param>
        /// <typeparam name="TDbContext">Db context type</typeparam>
        /// <returns></returns>
        public static IServiceCollection AddColumnChangesLogger<TDbContext>(this IServiceCollection services)
            where TDbContext : class, IUploadQueueDbContext
        {
            services.AddScoped<IColumnChangesLogger, ColumnChangesLogger<TDbContext>>();
            return services;
        }
    }
}
