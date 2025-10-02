using BookingManagementService.DataAccess.Data;
using BookingManagementService.DataAccess.Entities;

namespace BookingManagementService.DataAccess.Repositories.interfaces
{
    public interface IAccountBalanceTransactionRepository
    {
        Task<AccountBalanceTransaction> FindByIdAsync(int id);
        Task<IEnumerable<AccountBalanceTransaction>> FindByTypesAndStatusesAndPeriodAsync(
            List<int>? transactionTypeIds,
            List<int>? transactionStatusIds,
            DateOnly startDate,
            DateOnly endDate);
    }
}
