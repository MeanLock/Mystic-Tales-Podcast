using TransactionService.DataAccess.Data;
using TransactionService.DataAccess.Entities;

namespace TransactionService.DataAccess.Repositories.interfaces
{
    public interface IFilterTagRepository
    {
        Task<IEnumerable<FilterTag>> FindByTagTypeIdAsync(int tagTypeId);
        
    }
}
