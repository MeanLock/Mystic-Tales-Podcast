using SystemConfigurationService.DataAccess.Data;
using SystemConfigurationService.DataAccess.Entities;

namespace SystemConfigurationService.DataAccess.Repositories.interfaces
{
    public interface ISystemConfigProfileRepository
    {
        Task<SystemConfigProfile> FindActiveProfileAsync();
        
    }
}
