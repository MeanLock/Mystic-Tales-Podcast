using SubscriptionService.DataAccess.Data;
using SubscriptionService.DataAccess.Repositories.interfaces;
using Microsoft.EntityFrameworkCore;
using SubscriptionService.DataAccess.Entities;

namespace SubscriptionService.DataAccess.Repositories
{
    
    public class TakerTagFilterRepository : ITakerTagFilterRepository
    {
        private readonly AppDbContext _appDbContext;

        public TakerTagFilterRepository(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        public async Task UpdateAsync(TakerTagFilter takerTagFilter)
        {
            _appDbContext.TakerTagFilters.Update(takerTagFilter);
            await _appDbContext.SaveChangesAsync();
        }
    }
}
