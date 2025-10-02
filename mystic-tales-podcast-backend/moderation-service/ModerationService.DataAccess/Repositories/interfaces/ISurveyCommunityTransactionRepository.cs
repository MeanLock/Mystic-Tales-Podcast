using ModerationService.DataAccess.Data;
using ModerationService.DataAccess.Entities;

namespace ModerationService.DataAccess.Repositories.interfaces
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
