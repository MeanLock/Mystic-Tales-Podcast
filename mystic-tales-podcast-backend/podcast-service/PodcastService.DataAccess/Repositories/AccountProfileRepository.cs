using PodcastService.DataAccess.Data;
using PodcastService.DataAccess.Repositories.interfaces;
using Microsoft.EntityFrameworkCore;
using PodcastService.DataAccess.Entities;
using PodcastService.Common.AppConfigurations.App.interfaces;

namespace PodcastService.DataAccess.Repositories
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
