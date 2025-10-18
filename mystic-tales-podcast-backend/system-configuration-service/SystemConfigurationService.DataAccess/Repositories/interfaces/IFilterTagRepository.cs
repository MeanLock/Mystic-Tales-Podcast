using SystemConfigurationService.DataAccess.Data;
using SystemConfigurationService.DataAccess.Entities;

namespace SystemConfigurationService.DataAccess.Repositories.interfaces
{
    public interface IFilterTagRepository
    {
        Task<IEnumerable<FilterTag>> FindByTagTypeIdAsync(int tagTypeId);
        
    }
}
