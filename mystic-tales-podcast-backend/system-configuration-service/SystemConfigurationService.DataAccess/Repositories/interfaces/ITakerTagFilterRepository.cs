using SystemConfigurationService.DataAccess.Data;
using SystemConfigurationService.DataAccess.Entities;

namespace SystemConfigurationService.DataAccess.Repositories.interfaces
{
    public interface ITakerTagFilterRepository
    {
        Task UpdateAsync(TakerTagFilter takerTagFilter);
        
    }
}
