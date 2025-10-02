using SubscriptionService.DataAccess.Data;
using SubscriptionService.DataAccess.Repositories.interfaces;
using Microsoft.EntityFrameworkCore;
using SubscriptionService.DataAccess.Entities;
using SubscriptionService.Common.AppConfigurations.App.interfaces;

namespace SubscriptionService.DataAccess.Repositories
{
    public class AccountProfileRepository : IAccountProfileRepository
    {
        private readonly AppDbContext _appDbContext;
        private readonly IAppConfig _appConfig;

        public AccountProfileRepository(AppDbContext appDbContext, IAppConfig appConfig)
        {
            _appConfig = appConfig;

            _appDbContext = appDbContext;
        }

        public async Task<AccountProfile> FindByAccountIdAsync(int accountId)
        {
            return await _appDbContext.AccountProfiles
                .Include(accountProfile => accountProfile.Account)
                .FirstOrDefaultAsync(accountProfile => accountProfile.AccountId == accountId);
        }

        public async Task UpdateAsync(AccountProfile accountProfile)
        {
            _appDbContext.AccountProfiles.Update(accountProfile);
            await _appDbContext.SaveChangesAsync();
        }
    }
}
