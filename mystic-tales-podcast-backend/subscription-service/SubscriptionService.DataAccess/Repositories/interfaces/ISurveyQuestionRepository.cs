using SubscriptionService.BusinessLogic.DTOs.Survey.Filters;
using SubscriptionService.DataAccess.Data;
using SubscriptionService.DataAccess.Entities;

namespace SubscriptionService.DataAccess.Repositories.interfaces
{
    public interface ISurveyQuestionRepository
    {
        Task<IEnumerable<SurveyQuestion>> FindBySurveyIdAndIsDeletedContainAsync(int surveyId, bool isDeletedContain);

        Task DeleteByIdAsync(Guid id, DateTime? deletedAt = null);
    }
}
