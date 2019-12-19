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
    public abstract class NextApiUnitOfWork<TDbContext> : INextApiUnitOfWork
        where TDbContext : class, INextApiDbContext
    {
        /// <summary>
        /// Accessor to DbContext
        /// </summary>
        protected readonly TDbContext Context;

        /// <summary>
        /// Indicates that repository sends DbTablesUpdatedEvent after commit 
        /// </summary>
        public bool SendUpdateEventAfterCommit { get; set; } = true;

        private INextApiEventManager _eventManager;


        /// <inheritdoc />
        protected NextApiUnitOfWork(TDbContext context, INextApiEventManager eventManager)
        {
            Context = context ?? throw new ArgumentNullException(nameof(context));
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

            await Context.SaveChangesAsync();

            if (SendUpdateEventAfterCommit)
                await RaiseUpdateEvent(changedTables);
        }

        private string[] CollectChanges()
        {
            if (!(Context is DbContext db))
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
