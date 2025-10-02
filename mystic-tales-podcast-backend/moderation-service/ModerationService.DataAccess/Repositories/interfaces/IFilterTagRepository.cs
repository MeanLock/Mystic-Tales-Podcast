using ModerationService.DataAccess.Data;
using ModerationService.DataAccess.Entities;

namespace ModerationService.DataAccess.Repositories.interfaces
{
    public interface IFilterTagRepository
    {
        Task<IEnumerable<FilterTag>> FindByTagTypeIdAsync(int tagTypeId);
        
    }
}
