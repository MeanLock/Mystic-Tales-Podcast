using System.Linq.Expressions;
using UserService.DataAccess.Entities.SqlServer;

namespace UserService.DataAccess.Repositories.interfaces
{
    public interface IAccountSavedPodcastEpisodeRepository
    {
        Task DeleteByAccountIdAndPodcastEpisodeIdAsync(int accountId, Guid podcastEpisodeId);
        Task<List<int>> DeleteByPodcastEpisodeIdAsync(Guid podcastEpisodeId);
    }
}
