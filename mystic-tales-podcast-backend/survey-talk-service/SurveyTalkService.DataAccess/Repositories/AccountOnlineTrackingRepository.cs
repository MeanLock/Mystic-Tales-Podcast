using SurveyTalkService.DataAccess.Data;
using SurveyTalkService.DataAccess.Repositories.interfaces;
using Microsoft.EntityFrameworkCore;
using SurveyTalkService.DataAccess.Entities;
using SurveyTalkService.Common.AppConfigurations.App.interfaces;

namespace SurveyTalkService.DataAccess.Repositories
{
    public class AccountOnlineTrackingRepository : IAccountOnlineTrackingRepository
    {
        private readonly AppDbContext _appDbContext;
        private readonly IAppConfig _appConfig;

        public AccountOnlineTrackingRepository(AppDbContext appDbContext, IAppConfig appConfig)
        {
            _appConfig = appConfig;

            _appDbContext = appDbContext;
        }

        public async Task<IEnumerable<AccountOnlineTracking>> FindByAccountIdsAndDateAsync(List<int> accountId, DateOnly date)
        {
            return await _appDbContext.AccountOnlineTrackings
                .Include(accountOnlineTracking => accountOnlineTracking.Account)
                .Where(accountOnlineTracking => accountId.Contains(accountOnlineTracking.AccountId) && accountOnlineTracking.OnlineDate == date)
                .ToListAsync();
        }

        public async Task<IEnumerable<IEnumerable<AccountOnlineTracking>>> FindByAccountIdsAndDatePeriodAsync(List<int> accountId, DateOnly startDate, DateOnly endDate)
        {
            var accountOnlineTrackings = await _appDbContext.AccountOnlineTrackings
                .Include(accountOnlineTracking => accountOnlineTracking.Account)
                .Where(accountOnlineTracking => accountId.Contains(accountOnlineTracking.AccountId) && 
                                                accountOnlineTracking.OnlineDate >= startDate && 
                                                accountOnlineTracking.OnlineDate <= endDate)
                .ToListAsync();

            return accountId.Select(id => accountOnlineTrackings.Where(aot => aot.AccountId == id));
        }

        public async Task<AccountOnlineTracking> FindLatestByAccountIdAsync(int accountId)
        {
            return await _appDbContext.AccountOnlineTrackings
                .Include(accountOnlineTracking => accountOnlineTracking.Account)
                .Where(accountOnlineTracking => accountOnlineTracking.AccountId == accountId)
                .OrderByDescending(accountOnlineTracking => accountOnlineTracking.OnlineDate)
                .FirstOrDefaultAsync();
        }

    }
}
