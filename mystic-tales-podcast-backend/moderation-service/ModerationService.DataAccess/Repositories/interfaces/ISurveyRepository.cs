using ModerationService.BusinessLogic.DTOs.Survey.Filters;
using ModerationService.DataAccess.Data;
using ModerationService.DataAccess.Entities;

namespace ModerationService.DataAccess.Repositories.interfaces
{
    public interface ISurveyRepository
    {
        Task<Survey> FindByIdAsync(int id);
        Task<Survey> FindByIdAndFilterObjectAsync(int id, SurveyFilterObject surveyFilterObject);
        Task<SurveyStatusTracking> GetLatestSurveyStatusTrackingBySurveyIdAsync(int surveyId);
        Task<IEnumerable<Survey>> FindByFilterObjectAsync(SurveyFilterObject surveyFilterObject);

        // Task<decimal> GetProfitByTypeAndPeriodAsync(int surveyTypeId, DateOnly startDate, DateOnly endDate);
        
    }
}
