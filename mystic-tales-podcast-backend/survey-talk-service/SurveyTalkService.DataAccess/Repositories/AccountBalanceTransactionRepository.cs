using SurveyTalkService.DataAccess.Data;
using SurveyTalkService.DataAccess.Repositories.interfaces;
using Microsoft.EntityFrameworkCore;
using SurveyTalkService.DataAccess.Entities;
using SurveyTalkService.BusinessLogic.DTOs.Survey.Filters;

namespace SurveyTalkService.DataAccess.Repositories
{
    public class AccountBalanceTransactionRepository : IAccountBalanceTransactionRepository
    {
        private readonly AppDbContext _appDbContext;

        public AccountBalanceTransactionRepository(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        public async Task<AccountBalanceTransaction> FindByIdAsync(int id)
        {
            return await _appDbContext.AccountBalanceTransactions
                .Include(ph => ph.Account)
                .Include(ph => ph.TransactionType)
                .Include(ph => ph.TransactionStatus)
                .FirstOrDefaultAsync(ph => ph.Id == id);
        }

        public async Task<IEnumerable<AccountBalanceTransaction>> FindByTypesAndStatusesAndPeriodAsync(
            List<int>? transactionTypeIds,
            List<int>? transactionStatusIds,
            DateOnly startDate,
            DateOnly endDate)
        {
            var query = _appDbContext.AccountBalanceTransactions.AsQueryable();

            if (transactionTypeIds != null && transactionTypeIds.Count > 0)
            {
                query = query.Where(ph => transactionTypeIds.Contains(ph.TransactionTypeId));
            }
            // Nếu null hoặc rỗng thì không lọc theo type

            if (transactionStatusIds != null && transactionStatusIds.Count > 0)
            {
                query = query.Where(ph => transactionStatusIds.Contains(ph.TransactionStatusId));
            }
            // Nếu null hoặc rỗng thì không lọc theo status

            query = query.Where(ph => DateOnly.FromDateTime(ph.CreatedAt.Date) >= startDate &&
                                      DateOnly.FromDateTime(ph.CreatedAt.Date) <= endDate);

            return await query.ToListAsync();
        }
    }
}
