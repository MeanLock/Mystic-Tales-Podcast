using SurveyTalkService.DataAccess.Data;
using SurveyTalkService.DataAccess.Entities;

namespace SurveyTalkService.DataAccess.Repositories.interfaces
{
    public interface IAccountProfileRepository
    {
        Task<AccountProfile> FindByAccountIdAsync(int accountId);
        Task UpdateAsync(AccountProfile accountProfile);
    }
}
