using SurveyTalkService.BusinessLogic.DTOs.Survey.Filters;
using SurveyTalkService.DataAccess.Data;
using SurveyTalkService.DataAccess.Entities;

namespace SurveyTalkService.DataAccess.Repositories.interfaces
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
