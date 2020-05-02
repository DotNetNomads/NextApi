using System.Threading.Tasks;

namespace NextApi.Common.Abstractions.DAL
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
