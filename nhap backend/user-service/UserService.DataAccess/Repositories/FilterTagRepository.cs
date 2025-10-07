using UserService.DataAccess.Data;
using UserService.DataAccess.Repositories.interfaces;
using Microsoft.EntityFrameworkCore;
using UserService.DataAccess.Entities;

namespace UserService.DataAccess.Repositories
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
