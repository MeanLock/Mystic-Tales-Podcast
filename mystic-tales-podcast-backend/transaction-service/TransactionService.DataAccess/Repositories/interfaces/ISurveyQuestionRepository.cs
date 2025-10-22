using TransactionService.BusinessLogic.DTOs.Survey.Filters;
using TransactionService.DataAccess.Data;
using TransactionService.DataAccess.Entities;

namespace TransactionService.DataAccess.Repositories.interfaces
{
    public interface ISurveyQuestionRepository
    {
        Task<IEnumerable<SurveyQuestion>> FindBySurveyIdAndIsDeletedContainAsync(int surveyId, bool isDeletedContain);

        Task DeleteByIdAsync(Guid id, DateTime? deletedAt = null);
    }
}
