using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using NextApi.Common.Abstractions.Security;
using NextApi.Common.Entity;
using NextApi.Server.EfCore.Tests.Entity;
using NextApi.Server.UploadQueue.DAL;

namespace NextApi.Server.EfCore.Tests.Base
{
    public class TestDbContext : UploadQueueDbContext
    {

        public DbSet<TestEntity> TestEntities { get; set; }
        public DbSet<TestEntityKeyPredicate> TestEntityKeyPredicates { get; set; }
        public DbSet<TestSoftDeletableEntity> TestSoftDeletableEntities { get; set; }
        public DbSet<TestLoggedSoftDeletableEntity> TestLoggedSoftDeletableEntities { get; set; }
        public DbSet<TestAuditEntity> TestAuditEntities { get; set; }
        public DbSet<TestColumnChangesEnabledEntity> TestColumnChangesEnabledEntities { get; set; }

        public TestDbContext(DbContextOptions options, INextApiUserAccessor nextApiUserAccessor) : base(options, nextApiUserAccessor)
        {
        }

        protected override void RecordAuditInfo(string subjectId, EntityEntry entityEntry)
        {
            if (string.IsNullOrWhiteSpace(subjectId))
                return;

            if (!(entityEntry.Entity is ILoggedEntity<int?> || entityEntry.Entity is ILoggedSoftDeletableEntity<int?>))
                return;

            var res = int.TryParse(subjectId, out var subjectIdParsed);
            if (!res) return;
            switch (entityEntry.State)
            {
                case EntityState.Modified:
                    switch (entityEntry.Entity)
                    {
                        case ILoggedSoftDeletableEntity<int?> loggedSoftDeletableEntity:
                            if (loggedSoftDeletableEntity.IsRemoved && !loggedSoftDeletableEntity.RemovedById.HasValue)
                                loggedSoftDeletableEntity.RemovedById = subjectIdParsed;
                            if (loggedSoftDeletableEntity.IsRemoved && !loggedSoftDeletableEntity.Removed.HasValue)
                                loggedSoftDeletableEntity.Removed = DateTimeOffset.Now;
                            break;
                        case ILoggedEntity<int?> entity:
                            entity.UpdatedById = subjectIdParsed;
                            entity.Updated = DateTimeOffset.Now;
                            break;
                    }
                    break;
                case EntityState.Added:
                    {
                        switch (entityEntry.Entity)
                        {
                            case ILoggedEntity<int?> entity:
                                if (!entity.CreatedById.HasValue)
                                    entity.CreatedById = subjectIdParsed;
                                if (!entity.Created.HasValue)
                                    entity.Created = DateTimeOffset.Now;
                                break;
                        }
                        break;
                    }
            }
        }
    }
}
