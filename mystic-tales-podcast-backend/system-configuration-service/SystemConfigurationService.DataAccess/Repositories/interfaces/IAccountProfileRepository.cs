using SystemConfigurationService.DataAccess.Data;
using SystemConfigurationService.DataAccess.Entities;

namespace SystemConfigurationService.DataAccess.Repositories.interfaces
{
    public interface IAccountProfileRepository
    {
        Task<AccountProfile> FindByAccountIdAsync(int accountId);
        Task UpdateAsync(AccountProfile accountProfile);
    }
}
