using System.Threading.Tasks;

namespace Abitech.NextApi.Common.Abstractions
{
    /// <summary>
    /// Represents unit of work commit manager
    /// </summary>
    public interface IUnitOfWork
    {
        /// <summary>
        /// Commits changes into db
        /// </summary>
        /// <returns></returns>
        Task CommitAsync();
    }
}
