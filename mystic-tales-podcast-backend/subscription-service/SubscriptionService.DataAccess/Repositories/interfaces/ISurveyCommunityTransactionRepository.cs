using SubscriptionService.DataAccess.Data;
using SubscriptionService.DataAccess.Entities;

namespace SubscriptionService.DataAccess.Repositories.interfaces
{
    public interface ISurveyCommunityTransactionRepository
    {
        Task<SurveyCommunityTransaction> FindByIdAsync(int id);
        Task<IEnumerable<SurveyCommunityTransaction>> FindByTypesAndStatusesAndPeriodAsync(
            List<int>? transactionTypeIds,
            List<int>? transactionStatusIds,
            DateOnly startDate,
            DateOnly endDate);

        Task<decimal> GetProfitByTypesAndStatusesAndPeriodAsync(
            List<int>? transactionTypeIds,
            List<int>? transactionStatusIds,
            DateOnly startDate,
            DateOnly endDate);
    }
}
