using SystemConfigurationService.DataAccess.Data;
using SystemConfigurationService.DataAccess.Entities;

namespace SystemConfigurationService.DataAccess.Repositories.interfaces
{
    public interface ISurveyTagFilterRepository
    {
        Task<IEnumerable<SurveyTagFilter>> FindBySurveyIdAsync(int surveyId);
        Task UpdateAsync(SurveyTagFilter surveyTagFilter);
        
    }
}
