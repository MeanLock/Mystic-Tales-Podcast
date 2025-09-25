using SurveyTalkService.DataAccess.Data;
using SurveyTalkService.DataAccess.Entities;

namespace SurveyTalkService.DataAccess.Repositories.interfaces
{
    public interface ISurveyTopicFavoriteRepository
    {
        Task<IEnumerable<SurveyTopicFavorite>> FindByAccountIdAsync(int accountId);
        Task<bool> DeleteByAccountIdAsync(int accountId);
        
    }
}
