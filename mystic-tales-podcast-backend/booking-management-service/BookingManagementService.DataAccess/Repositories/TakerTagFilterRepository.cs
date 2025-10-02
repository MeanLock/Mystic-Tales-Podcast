using BookingManagementService.DataAccess.Data;
using BookingManagementService.DataAccess.Repositories.interfaces;
using Microsoft.EntityFrameworkCore;
using BookingManagementService.DataAccess.Entities;

namespace BookingManagementService.DataAccess.Repositories
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
