using System.Data;
using Abitech.NextApi.Common.Abstractions;
using Abitech.NextApi.Server.Common;
using Abitech.NextApi.Server.UploadQueue.ChangeTracking;
using Abitech.NextApi.Server.UploadQueue.DAL;
using Abitech.NextApi.Server.UploadQueue.Service;
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

        /// <summary>
        /// Add UploadQueue service to NextApi
        /// </summary>
        /// <param name="serverBuilder"></param>
        /// <param name="serviceName"></param>
        /// <param name="uploadQueueModelsAssemblyName"></param>
        /// <returns></returns>
        public static NextApiServiceBuilder AddUploadQueueService(this NextApiBuilder serverBuilder, string serviceName,
            string uploadQueueModelsAssemblyName)
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
