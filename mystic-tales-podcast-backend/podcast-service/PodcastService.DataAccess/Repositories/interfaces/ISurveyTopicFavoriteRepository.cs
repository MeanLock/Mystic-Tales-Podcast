using PodcastService.DataAccess.Data;
using PodcastService.DataAccess.Entities;

namespace PodcastService.DataAccess.Repositories.interfaces
{
    public interface ISurveyTopicFavoriteRepository
    {
        Task<IEnumerable<SurveyTopicFavorite>> FindByAccountIdAsync(int accountId);
        Task<bool> DeleteByAccountIdAsync(int accountId);
        
    }
}
