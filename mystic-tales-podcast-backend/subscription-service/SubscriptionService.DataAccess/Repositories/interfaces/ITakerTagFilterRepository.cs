using SubscriptionService.DataAccess.Data;
using SubscriptionService.DataAccess.Entities;

namespace SubscriptionService.DataAccess.Repositories.interfaces
{
    public interface ITakerTagFilterRepository
    {
        Task UpdateAsync(TakerTagFilter takerTagFilter);
        
    }
}
