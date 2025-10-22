using ModerationService.DataAccess.Data;
using ModerationService.DataAccess.Entities;

namespace ModerationService.DataAccess.Repositories.interfaces
{
    public interface IAccountProfileRepository
    {
        Task<AccountProfile> FindByAccountIdAsync(int accountId);
        Task UpdateAsync(AccountProfile accountProfile);
    }
}
