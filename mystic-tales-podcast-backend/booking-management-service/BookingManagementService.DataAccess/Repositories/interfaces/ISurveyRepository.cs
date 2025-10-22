using BookingManagementService.BusinessLogic.DTOs.Survey.Filters;
using BookingManagementService.DataAccess.Data;
using BookingManagementService.DataAccess.Entities;

namespace BookingManagementService.DataAccess.Repositories.interfaces
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
