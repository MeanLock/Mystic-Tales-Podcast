using SurveyTalkService.DataAccess.Data;
using SurveyTalkService.DataAccess.Entities;

namespace SurveyTalkService.DataAccess.Repositories.interfaces
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
