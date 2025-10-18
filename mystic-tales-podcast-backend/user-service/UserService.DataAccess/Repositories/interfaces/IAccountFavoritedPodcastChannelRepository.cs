using System.Linq.Expressions;
using UserService.DataAccess.Entities.SqlServer;

namespace UserService.DataAccess.Repositories.interfaces
{
    public interface IAccountFavoritedPodcastChannelRepository
    {
        Task DeleteByAccountIdAndPodcastChannelIdAsync(int accountId, Guid podcastChannelId);
        Task<List<int>> DeleteByPodcastChannelIdAsync(Guid podcastChannelId);
    }
}
