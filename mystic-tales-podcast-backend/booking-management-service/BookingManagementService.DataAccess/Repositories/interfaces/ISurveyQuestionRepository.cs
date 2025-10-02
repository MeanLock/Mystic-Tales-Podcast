using BookingManagementService.BusinessLogic.DTOs.Survey.Filters;
using BookingManagementService.DataAccess.Data;
using BookingManagementService.DataAccess.Entities;

namespace BookingManagementService.DataAccess.Repositories.interfaces
{
    public interface ISurveyQuestionRepository
    {
        Task<IEnumerable<SurveyQuestion>> FindBySurveyIdAndIsDeletedContainAsync(int surveyId, bool isDeletedContain);

        Task DeleteByIdAsync(Guid id, DateTime? deletedAt = null);
    }
}
