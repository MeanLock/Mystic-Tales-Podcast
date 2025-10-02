using SubscriptionService.DataAccess.Data;
using SubscriptionService.DataAccess.Entities;

namespace SubscriptionService.DataAccess.Repositories.interfaces
{
    public interface ISurveyTakenResultTagFilterRepository
    {
        Task<IEnumerable<SurveyTakenResult>> FindLimitByTakerIdAsync(int takerId, int limit);
    }
}
