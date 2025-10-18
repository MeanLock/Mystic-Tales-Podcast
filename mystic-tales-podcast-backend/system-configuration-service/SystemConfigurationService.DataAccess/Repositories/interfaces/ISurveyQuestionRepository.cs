using SystemConfigurationService.BusinessLogic.DTOs.Survey.Filters;
using SystemConfigurationService.DataAccess.Data;
using SystemConfigurationService.DataAccess.Entities;

namespace SystemConfigurationService.DataAccess.Repositories.interfaces
{
    public interface ISurveyQuestionRepository
    {
        Task<IEnumerable<SurveyQuestion>> FindBySurveyIdAndIsDeletedContainAsync(int surveyId, bool isDeletedContain);

        Task DeleteByIdAsync(Guid id, DateTime? deletedAt = null);
    }
}
