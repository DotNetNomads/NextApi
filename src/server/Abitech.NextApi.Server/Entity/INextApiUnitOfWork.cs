using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Abitech.NextApi.Server.Entity
{
    /// <summary>
    /// Represents unit of work commit manager
    /// </summary>
    public interface INextApiUnitOfWork
    {
        /// <summary>
        /// Commits changes into db
        /// </summary>
        /// <returns></returns>
        Task CommitAsync();
    }
}
