using TransactionService.DataAccess.Data;
using TransactionService.DataAccess.Entities;

namespace TransactionService.DataAccess.Repositories.interfaces
{
    public interface ISurveyTopicFavoriteRepository
    {
        Task<IEnumerable<SurveyTopicFavorite>> FindByAccountIdAsync(int accountId);
        Task<bool> DeleteByAccountIdAsync(int accountId);
        
    }
}
