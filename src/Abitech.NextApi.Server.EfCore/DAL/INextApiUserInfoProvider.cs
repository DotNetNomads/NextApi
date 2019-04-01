using System.Threading.Tasks;

namespace Abitech.NextApi.Server.EfCore.DAL
{
    /// <summary>
    /// Used for user info accessing (example: from db context, for logging, etc.)
    /// </summary>
    public interface INextApiUserInfoProvider
    {
        Task<int?> CurrentUserId();
    }
}
