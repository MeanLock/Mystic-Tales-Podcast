using UserService.DataAccess.Data;
using UserService.DataAccess.Entities;

namespace UserService.DataAccess.Repositories.interfaces
{
    public interface ISurveyTopicFavoriteRepository
    {
        Task<IEnumerable<SurveyTopicFavorite>> FindByAccountIdAsync(int accountId);
        Task<bool> DeleteByAccountIdAsync(int accountId);
        
    }
}
