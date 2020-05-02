using System;
using NextApi.Common.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace NextApi.Server.EfCore.DAL
{
    /// <summary>
    /// Functional helpers for NextApi DbContext
    /// </summary>
    public static class NextApiDbHelpers
    {
        /// <summary>
        /// Records audit info about user who created the entity
        /// </summary>
        /// <param name="context"></param>
        /// <param name="userId"></param>
        /// <param name="entityEntry"></param>
        public static void RecordAuditInfo(this NextApiDbContext context, int? userId, EntityEntry entityEntry)
        {
            if (!(entityEntry.Entity is ILoggedEntity entity))
                return;

            switch (entityEntry.State)
            {
                case EntityState.Modified:
                    entity.UpdatedById = userId;
                    entity.Updated = DateTimeOffset.Now;
                    break;
                case EntityState.Added:
                {
                    if (!entity.CreatedById.HasValue)
                        entity.CreatedById = userId;
                    if (!entity.Created.HasValue)
                        entity.Created = DateTimeOffset.Now;
                    break;
                }
            }
        }
    }
}
