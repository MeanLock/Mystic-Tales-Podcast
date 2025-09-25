using SurveyTalkService.DataAccess.Data;
using SurveyTalkService.DataAccess.Entities;

namespace SurveyTalkService.DataAccess.Repositories.interfaces
{
    public interface ISurveyTagFilterRepository
    {
        Task<IEnumerable<SurveyTagFilter>> FindBySurveyIdAsync(int surveyId);
        Task UpdateAsync(SurveyTagFilter surveyTagFilter);
        
    }
}
