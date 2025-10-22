using BookingManagementService.DataAccess.Data;
using BookingManagementService.DataAccess.Repositories.interfaces;
using Microsoft.EntityFrameworkCore;
using BookingManagementService.DataAccess.Entities;

namespace BookingManagementService.DataAccess.Repositories
{
    public class FilterTagRepository : IFilterTagRepository
    {
        private readonly AppDbContext _appDbContext;

        public FilterTagRepository(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        public async Task<IEnumerable<FilterTag>> FindByTagTypeIdAsync(int tagTypeId)
        {
            return await _appDbContext.FilterTags
                .Include(ft => ft.FilterTagType)
                .Where(ft => ft.FilterTagTypeId == tagTypeId)
                .ToListAsync();
        }
    }
}
