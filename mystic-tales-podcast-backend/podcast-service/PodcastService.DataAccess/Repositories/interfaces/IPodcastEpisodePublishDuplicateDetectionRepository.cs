using System.Linq.Expressions;

namespace PodcastService.DataAccess.Repositories.interfaces
{
    public interface IPodcastEpisodePublishDuplicateDetectionRepository
    {
        Task<bool> DeleteByPublishReviewSessionIdAsync(int sessionId);
    }
}
