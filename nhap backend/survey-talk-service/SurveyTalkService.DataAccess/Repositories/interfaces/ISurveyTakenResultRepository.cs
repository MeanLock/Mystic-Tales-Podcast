using SurveyTalkService.DataAccess.Data;
using SurveyTalkService.DataAccess.Entities;

namespace SurveyTalkService.DataAccess.Repositories.interfaces
{
    public interface ISurveyTakenResultRepository
    {
        Task<IEnumerable<SurveyTakenResult>> FindByAccountIdsAndDateAsync(List<int> accountIds, DateOnly date, bool? isInvalidTakenResultContain = true);
        Task<IEnumerable<SurveyTakenResult>> FindByAccountIdsAndDatePeriodAsync(List<int> accountIds, DateOnly startDate, DateOnly endDate, bool? isInvalidTakenResultContain = true);
        Task<IEnumerable<SurveyTakenResult>> FindBySurveyIdAsync(int surveyId, bool? IsInvalidTakenResultContain = null);
        Task<IEnumerable<SurveyTakenResult>> FindByTakerIdAsync(int takerId, int? limit = null);
        Task<SurveyTakenResult> FindByTakerIdAndSurveyIdAsync(int takerId, int surveyId);
        Task<int> CountBySurveyIdAsync(int surveyId, bool? isInvalidTakenResultContain = null);
    }
}
