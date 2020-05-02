using NextApi.Common.DTO;
using NextApi.Common.Entity;
using NextApi.Common.Tree;
using NextApi.Server.Common;
using NextApi.Server.Entity;

namespace NextApi.Server.Service
{
    /// <summary>
    /// Extensions for NextApiBuilder
    /// </summary>
    public static class NextApiBuilderExtensions
    {
        /// <summary>
        /// Add default NextApi entity service for entity type
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="serviceName">Service name (optional, by default is it entity name)</param>
        /// <typeparam name="TEntityDto">DTO type for entity</typeparam>
        /// <typeparam name="TEntity">Entity type</typeparam>
        /// <typeparam name="TKey">Entity key type</typeparam>
        /// <returns>Service builder for detailed configuration</returns>
        public static NextApiServiceBuilder AddEntityService<TEntityDto,
            TEntity, TKey>(this NextApiBuilder builder,
            string serviceName = null)
            where TEntityDto : class, IEntityDto<TKey>
            where TEntity : class, IEntity<TKey>
        {
            if (string.IsNullOrWhiteSpace(serviceName))
                serviceName = typeof(TEntity).Name;
            return builder.AddService<NextApiEntityService<TEntityDto, TEntity, TKey>>(serviceName);
        }

        /// <summary>
        /// Add default NextApi entity service for tree-type entity type
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="serviceName">Service name (optional, by default is it entity name)</param>
        /// <typeparam name="TEntityDto">DTO type for entity</typeparam>
        /// <typeparam name="TEntity">Entity type</typeparam>
        /// <typeparam name="TKey">Entity key type</typeparam>
        /// <typeparam name="TParentKey">Parent entity key</typeparam>
        /// <returns>Service builder for detailed configuration</returns>
        public static NextApiServiceBuilder AddTreeEntityService<TEntityDto,
            TEntity, TKey, TParentKey>(this NextApiBuilder builder,
            string serviceName = null)
            where TEntityDto : class, ITreeEntityDto<TKey, TParentKey>
            where TEntity : class, ITreeEntity<TKey, TParentKey>
        {
            if (string.IsNullOrWhiteSpace(serviceName))
                serviceName = typeof(TEntity).Name;
            return builder.AddService<NextApiTreeEntityService<TEntityDto, TEntity, TKey, TParentKey>>(serviceName);
        }
    }
}
