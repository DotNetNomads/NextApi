using System;
using System.Threading.Tasks;
using Abitech.NextApi.Server.Entity;

namespace Abitech.NextApi.Server.EfCore.DAL
{
    /// <summary>
    /// Unit for work implementation
    /// </summary>
    public abstract class NextApiUnitOfWork<TDbContext> : INextApiUnitOfWork
    where TDbContext: class, INextApiDbContext
    {
        /// <summary>
        /// Accessor to DbContext
        /// </summary>
        protected TDbContext Context;

        /// <inheritdoc />
        protected NextApiUnitOfWork(TDbContext context)
        {
            Context = context ?? throw new ArgumentNullException(nameof(context));
        }

        /// <summary>
        /// Save all changes
        /// </summary>
        /// <returns></returns>
        public async Task Commit()
        {
            await Context.SaveChangesAsync();
        }
    }
}
