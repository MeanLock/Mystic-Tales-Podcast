using System.Linq.Expressions;

namespace PodcastService.DataAccess.Repositories.interfaces
{
    public interface IPodcastShowReviewRepository
    {
        Task<bool> DeleteByPodcastShowIdAsync(Guid showId);
    }
}
