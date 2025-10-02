using SubscriptionService.DataAccess.Data;
using SubscriptionService.DataAccess.Entities;

namespace SubscriptionService.DataAccess.Repositories.interfaces
{
    public interface IAccountProfileRepository
    {
        Task<AccountProfile> FindByAccountIdAsync(int accountId);
        Task UpdateAsync(AccountProfile accountProfile);
    }
}
