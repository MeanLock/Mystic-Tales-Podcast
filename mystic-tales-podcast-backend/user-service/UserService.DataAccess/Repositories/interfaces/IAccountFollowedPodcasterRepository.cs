using System.Linq.Expressions;
using UserService.DataAccess.Entities.SqlServer;

namespace UserService.DataAccess.Repositories.interfaces
{
    public interface IAccountFollowedPodcasterRepository
    {
        Task DeleteByAccountIdAndPodcasterIdAsync(int accountId, int podcasterId);
    }
}
