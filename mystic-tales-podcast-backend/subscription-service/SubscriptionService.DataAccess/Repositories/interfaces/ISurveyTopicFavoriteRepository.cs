using SubscriptionService.DataAccess.Data;
using SubscriptionService.DataAccess.Entities;

namespace SubscriptionService.DataAccess.Repositories.interfaces
{
    public interface ISurveyTopicFavoriteRepository
    {
        Task<IEnumerable<SurveyTopicFavorite>> FindByAccountIdAsync(int accountId);
        Task<bool> DeleteByAccountIdAsync(int accountId);
        
    }
}
