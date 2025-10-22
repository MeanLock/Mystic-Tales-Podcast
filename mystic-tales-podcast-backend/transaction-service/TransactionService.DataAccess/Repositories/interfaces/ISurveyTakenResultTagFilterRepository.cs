using TransactionService.DataAccess.Data;
using TransactionService.DataAccess.Entities;

namespace TransactionService.DataAccess.Repositories.interfaces
{
    public interface ISurveyTakenResultTagFilterRepository
    {
        Task<IEnumerable<SurveyTakenResult>> FindLimitByTakerIdAsync(int takerId, int limit);
    }
}
