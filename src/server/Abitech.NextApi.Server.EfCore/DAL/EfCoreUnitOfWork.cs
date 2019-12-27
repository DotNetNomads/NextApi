using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abitech.NextApi.Common.Abstractions;
using Abitech.NextApi.Common.Event.System;
using Microsoft.EntityFrameworkCore;

namespace Abitech.NextApi.Server.EfCore.DAL
{
    /// <summary>
    /// Unit for work implementation
    /// </summary>
    public class EfCoreUnitOfWork : IUnitOfWork
    {
        /// <summary>
        /// Accessor to DbContext
        /// </summary>
        private readonly INextApiDbContext _сontext;

        /// <summary>
        /// Indicates that repository sends DbTablesUpdatedEvent after commit 
        /// </summary>
        public bool SendUpdateEventAfterCommit { get; set; } = true;

        private readonly INextApiEventManager _eventManager;


        /// <inheritdoc />
        public EfCoreUnitOfWork(INextApiDbContext context, INextApiEventManager eventManager)
        {
            _сontext = context ?? throw new ArgumentNullException(nameof(context));
            _eventManager = eventManager;
        }

        /// <summary>
        /// Save all changes
        /// </summary>
        /// <returns></returns>
        public virtual async Task CommitAsync()
        {
            string[] changedTables = null;
            if (SendUpdateEventAfterCommit)
                changedTables = CollectChanges();

            await _сontext.SaveChangesAsync();

            if (SendUpdateEventAfterCommit)
                await RaiseUpdateEvent(changedTables);
        }

        private string[] CollectChanges()
        {
            if (!(_сontext is DbContext db))
                throw new InvalidOperationException("Context should be based on DbContext");

            return db.ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Modified || e.State == EntityState.Added ||
                            e.State == EntityState.Deleted).Select(e => e.Metadata.ShortName()).Distinct().ToArray();
        }

        private async Task RaiseUpdateEvent(string[] changedTables)
        {
            if (changedTables == null || !changedTables.Any())
                return;
            await _eventManager.Publish<DbTablesUpdatedEvent, string[]>(changedTables);
        }
    }
}
