using SubscriptionService.DataAccess.Data;
using SubscriptionService.DataAccess.Entities;

namespace SubscriptionService.DataAccess.Repositories.interfaces
{
    public interface ISurveyTagFilterRepository
    {
        Task<IEnumerable<SurveyTagFilter>> FindBySurveyIdAsync(int surveyId);
        Task UpdateAsync(SurveyTagFilter surveyTagFilter);
        
    }
}
