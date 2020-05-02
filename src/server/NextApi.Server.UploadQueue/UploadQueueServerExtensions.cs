using Microsoft.Extensions.DependencyInjection;
using NextApi.Common.Abstractions.DAL;
using NextApi.Server.Common;
using NextApi.Server.UploadQueue.ChangeTracking;
using NextApi.Server.UploadQueue.DAL;
using NextApi.Server.UploadQueue.Service;

namespace NextApi.Server.UploadQueue
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

        /// <summary>
        /// Add UploadQueue service to NextApi
        /// </summary>
        /// <param name="serverBuilder"></param>
        /// <param name="serviceName"></param>
        /// <param name="uploadQueueModelsAssemblyName"></param>
        /// <returns></returns>
        public static NextApiServiceBuilder AddUploadQueueService(this NextApiBuilder serverBuilder,
            string uploadQueueModelsAssemblyName, string serviceName = null)
        {
            serverBuilder.ServiceCollection.AddTransient(c =>
                new UploadQueueService(
                    c.GetService<IColumnChangesLogger>(),
                    c.GetService<IUnitOfWork>(), c,
                    uploadQueueModelsAssemblyName));
            return serverBuilder.AddService<UploadQueueService>(serviceName, false);
        }
    }
}
