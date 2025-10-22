using TransactionService.DataAccess.Data;
using TransactionService.DataAccess.Entities;

namespace TransactionService.DataAccess.Repositories.interfaces
{
    public interface ITakerTagFilterRepository
    {
        Task UpdateAsync(TakerTagFilter takerTagFilter);
        
    }
}
