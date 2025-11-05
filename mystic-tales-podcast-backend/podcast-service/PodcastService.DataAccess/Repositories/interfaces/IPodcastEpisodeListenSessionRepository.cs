using System.Linq.Expressions;

namespace PodcastService.DataAccess.Repositories.interfaces
{
    public interface IPodcastEpisodeListenSessionRepository
    {
        Task<bool> DeleteByPodcastEpisodeIdAsync(Guid episodeId);
        Task<bool> RemoveContentByPodcastEpisodeIdAsync(Guid episodeId);
    }
}
