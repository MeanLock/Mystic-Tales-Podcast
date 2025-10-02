using BookingManagementService.DataAccess.Data;
using BookingManagementService.DataAccess.Entities;

namespace BookingManagementService.DataAccess.Repositories.interfaces
{
    public interface ISystemConfigProfileRepository
    {
        Task<SystemConfigProfile> FindActiveProfileAsync();
        
    }
}
