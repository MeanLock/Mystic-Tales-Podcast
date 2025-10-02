using ModerationService.DataAccess.Data;
using ModerationService.DataAccess.Entities;

namespace ModerationService.DataAccess.Repositories.interfaces
{
    public interface IAccountOnlineTrackingRepository
    {
        Task<IEnumerable<AccountOnlineTracking>> FindByAccountIdsAndDateAsync(List<int> accountId, DateOnly date);
        Task<IEnumerable<IEnumerable<AccountOnlineTracking>>> FindByAccountIdsAndDatePeriodAsync(List<int> accountId, DateOnly startDate, DateOnly endDate);
        Task<AccountOnlineTracking> FindLatestByAccountIdAsync(int accountId);
    }
}
