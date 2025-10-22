using System.Linq.Expressions;

namespace PodcastService.DataAccess.Repositories.interfaces
{
    public interface IPodcastEpisodeIllegalContentTypeMarkingRepository
    {
        Task<bool> DeleteByPodcastEpisodeIdAsync(Guid episodeId);
    }
}
