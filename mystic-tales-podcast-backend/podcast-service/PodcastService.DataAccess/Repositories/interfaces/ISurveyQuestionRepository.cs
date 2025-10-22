using PodcastService.BusinessLogic.DTOs.Survey.Filters;
using PodcastService.DataAccess.Data;
using PodcastService.DataAccess.Entities;

namespace PodcastService.DataAccess.Repositories.interfaces
{
    public interface ISurveyQuestionRepository
    {
        Task<IEnumerable<SurveyQuestion>> FindBySurveyIdAndIsDeletedContainAsync(int surveyId, bool isDeletedContain);

        Task DeleteByIdAsync(Guid id, DateTime? deletedAt = null);
    }
}
