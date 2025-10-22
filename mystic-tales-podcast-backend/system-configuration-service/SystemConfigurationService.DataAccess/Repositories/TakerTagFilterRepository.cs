using SystemConfigurationService.DataAccess.Data;
using SystemConfigurationService.DataAccess.Repositories.interfaces;
using Microsoft.EntityFrameworkCore;
using SystemConfigurationService.DataAccess.Entities;

namespace SystemConfigurationService.DataAccess.Repositories
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
