using System.Linq.Expressions;
using UserService.DataAccess.Entities.SqlServer;

namespace UserService.DataAccess.Repositories.interfaces
{
    public interface IPodcastBuddyReviewRepository
    {
        Task<List<int>> DeleteByPodcasterIdAsync(int podcasterId);
    }
}
