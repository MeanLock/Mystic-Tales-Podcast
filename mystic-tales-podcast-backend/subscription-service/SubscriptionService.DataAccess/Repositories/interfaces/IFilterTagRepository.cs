using SubscriptionService.DataAccess.Data;
using SubscriptionService.DataAccess.Entities;

namespace SubscriptionService.DataAccess.Repositories.interfaces
{
    public interface IFilterTagRepository
    {
        Task<IEnumerable<FilterTag>> FindByTagTypeIdAsync(int tagTypeId);
        
    }
}
