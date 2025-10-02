using SystemConfigurationService.DataAccess.Data;
using SystemConfigurationService.DataAccess.Repositories.interfaces;
using Microsoft.EntityFrameworkCore;
using SystemConfigurationService.DataAccess.Entities;

namespace SystemConfigurationService.DataAccess.Repositories
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
