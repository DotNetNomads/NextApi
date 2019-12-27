using Abitech.NextApi.Common.DTO;
using Abitech.NextApi.Common.Entity;

namespace Abitech.NextApi.Server.Service
{
    /// <summary>
    /// Extensions for NextApiServerBuilder
    /// </summary>
    public static class NextApiServerBuilderExtensions
    {
        public static void AddEntityService<TEntityDto, TEntity, TKey>(this NextApiServerBuilder serverBuilder,
            string serviceName = null)
        where TEntityDto: IEntityDto<TKey>
        where TEntity: IEntity<TKey>
        {
            if (string.IsNullOrWhiteSpace(serviceName))
                serviceName = typeof(TEntity).Name;
            
        }
    }
}
