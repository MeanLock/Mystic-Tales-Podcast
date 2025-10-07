using UserService.DataAccess.Data;
using UserService.DataAccess.Entities;

namespace UserService.DataAccess.Repositories.interfaces
{
    public interface ISurveyTagFilterRepository
    {
        Task<IEnumerable<SurveyTagFilter>> FindBySurveyIdAsync(int surveyId);
        Task UpdateAsync(SurveyTagFilter surveyTagFilter);
        
    }
}
