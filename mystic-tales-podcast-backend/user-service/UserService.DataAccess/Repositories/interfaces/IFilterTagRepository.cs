using UserService.DataAccess.Data;
using UserService.DataAccess.Entities;

namespace UserService.DataAccess.Repositories.interfaces
{
    public interface IFilterTagRepository
    {
        Task<IEnumerable<FilterTag>> FindByTagTypeIdAsync(int tagTypeId);
        
    }
}
