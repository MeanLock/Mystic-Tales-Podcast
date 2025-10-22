using SystemConfigurationService.BusinessLogic.DTOs.Survey.Filters;
using SystemConfigurationService.DataAccess.Data;
using SystemConfigurationService.DataAccess.Entities;

namespace SystemConfigurationService.DataAccess.Repositories.interfaces
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
