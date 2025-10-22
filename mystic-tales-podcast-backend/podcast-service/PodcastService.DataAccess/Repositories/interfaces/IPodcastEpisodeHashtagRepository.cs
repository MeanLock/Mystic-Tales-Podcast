using System.Linq.Expressions;

namespace PodcastService.DataAccess.Repositories.interfaces
{
    public interface IPodcastEpisodeHashtagRepository
    {
        Task<bool> DeleteByPodcastEpisodeIdAsync(Guid episodeId);
    }
}
