using ModerationService.DataAccess.Data;
using ModerationService.DataAccess.Entities;

namespace ModerationService.DataAccess.Repositories.interfaces
{
    public interface ISystemConfigProfileRepository
    {
        Task<SystemConfigProfile> FindActiveProfileAsync();
        
    }
}
