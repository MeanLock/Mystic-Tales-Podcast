using System.Linq.Expressions;

namespace PodcastService.DataAccess.Repositories.interfaces
{
    public interface IPodcastShowHashtagRepository
    {
        Task<bool> DeleteByPodcastShowIdAsync(Guid showId);
    }
}
