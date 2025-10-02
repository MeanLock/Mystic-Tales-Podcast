using TransactionService.DataAccess.Data;
using TransactionService.DataAccess.Entities;

namespace TransactionService.DataAccess.Repositories.interfaces
{
    public interface ISystemConfigProfileRepository
    {
        Task<SystemConfigProfile> FindActiveProfileAsync();
        
    }
}
