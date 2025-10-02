using SubscriptionService.DataAccess.Data;
using SubscriptionService.DataAccess.Entities;

namespace SubscriptionService.DataAccess.Repositories.interfaces
{
    public interface ISurveyTakerSegmentRepository
    {
        Task UpdateAsync(int surveyId, SurveyTakerSegment surveyTakerSegment);
        
    }
}
