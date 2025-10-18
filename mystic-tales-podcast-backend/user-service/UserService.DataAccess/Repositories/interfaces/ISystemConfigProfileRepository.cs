using UserService.DataAccess.Data;
using UserService.DataAccess.Entities;

namespace UserService.DataAccess.Repositories.interfaces
{
    public interface ISystemConfigProfileRepository
    {
        Task<SystemConfigProfile> FindActiveProfileAsync();
        
    }
}
