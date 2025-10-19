using System.Linq.Expressions;
using UserService.DataAccess.Entities.SqlServer;

namespace UserService.DataAccess.Repositories.interfaces
{
    public interface IAccountFollowedPodcastShowRepository
    {
        Task DeleteByAccountIdAndPodcastShowIdAsync(int accountId, Guid podcastShowId);
        Task<List<int>> DeleteByPodcastShowIdAsync(Guid podcastShowId);
    }
}
