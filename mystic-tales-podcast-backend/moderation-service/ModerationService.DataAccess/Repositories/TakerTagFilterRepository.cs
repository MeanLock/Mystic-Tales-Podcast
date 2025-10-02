using ModerationService.DataAccess.Data;
using ModerationService.DataAccess.Repositories.interfaces;
using Microsoft.EntityFrameworkCore;
using ModerationService.DataAccess.Entities;

namespace ModerationService.DataAccess.Repositories
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
