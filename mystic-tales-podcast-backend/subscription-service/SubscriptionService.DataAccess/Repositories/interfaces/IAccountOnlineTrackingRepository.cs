using SubscriptionService.DataAccess.Data;
using SubscriptionService.DataAccess.Entities;

namespace SubscriptionService.DataAccess.Repositories.interfaces
{
    public interface IAccountOnlineTrackingRepository
    {
        Task<IEnumerable<AccountOnlineTracking>> FindByAccountIdsAndDateAsync(List<int> accountId, DateOnly date);
        Task<IEnumerable<IEnumerable<AccountOnlineTracking>>> FindByAccountIdsAndDatePeriodAsync(List<int> accountId, DateOnly startDate, DateOnly endDate);
        Task<AccountOnlineTracking> FindLatestByAccountIdAsync(int accountId);
    }
}
