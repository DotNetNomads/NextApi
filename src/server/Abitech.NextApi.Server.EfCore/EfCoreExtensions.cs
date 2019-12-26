using System;
using Abitech.NextApi.Common.Abstractions;
using Abitech.NextApi.Common.Entity;
using Abitech.NextApi.Server.EfCore.DAL;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Abitech.NextApi.Server.EfCore
{
    /// <summary>
    /// Extensions for EF Core NextApi integration
    /// </summary>
    public static class EfCoreExtensions
    {
        /// <summary>
        /// Add EF Core DB context as INextApiDbContext
        /// </summary>
        /// <param name="serviceCollection"></param>
        /// <param name="options">EF Core context options</param>
        /// <typeparam name="TDbContext">DbContext implementation type</typeparam>
        /// <typeparam name="TDbContextInterface">DbContext resolving (interface) type</typeparam>
        /// <returns></returns>
        public static IServiceCollection AddNextApiDbContext<TDbContextInterface, TDbContext>(
            this IServiceCollection serviceCollection, Action<DbContextOptionsBuilder> options)
            where TDbContext : DbContext, TDbContextInterface
            where TDbContextInterface : INextApiDbContext
        {
            return serviceCollection
                .AddDbContext<TDbContextInterface, TDbContext>(options)
                .AddScoped<INextApiDbContext>(c => c.GetService<TDbContextInterface>());
        }

        /// <summary>
        /// Add default implementation of the UnitOfWork for EF Core
        /// </summary>
        /// <param name="serviceCollection"></param>
        /// <returns></returns>
        public static IServiceCollection AddDefaultUnitOfWork(this IServiceCollection serviceCollection) =>
            serviceCollection.AddTransient<INextApiUnitOfWork, NextApiUnitOfWork>();

        /// <summary>
        /// Add custom implementation of the UnitOfWork for EF Core
        /// </summary>
        /// <param name="serviceCollection"></param>
        /// <typeparam name="TUnitOfWork">The UnitOfWork implementation type</typeparam>
        /// <returns></returns>
        public static IServiceCollection AddCustomUnitOfWork<TUnitOfWork>(this IServiceCollection serviceCollection)
            where TUnitOfWork : class, INextApiUnitOfWork =>
            serviceCollection.AddTransient<INextApiUnitOfWork, TUnitOfWork>();

        /// <summary>
        /// Add default repository for a Entity type
        /// </summary>
        /// <param name="serviceCollection"></param>
        /// <typeparam name="TEntity">Entity type</typeparam>
        /// <typeparam name="TKey">Entity type's key</typeparam>
        /// <returns></returns>
        public static IServiceCollection AddDefaultRepo<TEntity, TKey>(this IServiceCollection serviceCollection)
            where TEntity : class, IEntity<TKey> => serviceCollection
            .AddTransient<INextApiRepository<TEntity, TKey>, NextApiRepository<TEntity, TKey>>();

        /// <summary>
        /// Add custom repository for a Entity type
        /// </summary>
        /// <param name="serviceCollection"></param>
        /// <typeparam name="TEntity">Entity type</typeparam>
        /// <typeparam name="TKey">Entity type's key</typeparam>
        /// <typeparam name="TImplementation">Repository implementation</typeparam>
        /// <returns></returns>
        public static IServiceCollection AddCustomRepo<TEntity, TKey, TImplementation>(
            this IServiceCollection serviceCollection)
            where TEntity : class, IEntity<TKey> where TImplementation : class, INextApiRepository<TEntity, TKey> =>
            serviceCollection
                .AddTransient<INextApiRepository<TEntity, TKey>, TImplementation>();

        /// <summary>
        /// Add custom repository for a Entity type
        /// </summary>
        /// <param name="serviceCollection"></param>
        /// <typeparam name="TEntity">Entity type</typeparam>
        /// <typeparam name="TKey">Entity type's key</typeparam>
        /// <typeparam name="TImplementation">Repository implementation</typeparam>
        /// <typeparam name="TInterface">Repository interface</typeparam>
        /// <returns></returns>
        public static IServiceCollection AddCustomRepo<TEntity, TKey, TInterface, TImplementation>(
            this IServiceCollection serviceCollection)
            where TEntity : class, IEntity<TKey>
            where TImplementation : class, TInterface
            where TInterface : class, INextApiRepository<TEntity, TKey> =>
            serviceCollection
                .AddCustomRepo<TEntity, TKey, TImplementation>()
                .AddTransient<TInterface, TImplementation>();
    }
}
