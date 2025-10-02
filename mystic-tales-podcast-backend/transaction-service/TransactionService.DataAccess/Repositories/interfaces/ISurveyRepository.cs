using TransactionService.BusinessLogic.DTOs.Survey.Filters;
using TransactionService.DataAccess.Data;
using TransactionService.DataAccess.Entities;

namespace TransactionService.DataAccess.Repositories.interfaces
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
