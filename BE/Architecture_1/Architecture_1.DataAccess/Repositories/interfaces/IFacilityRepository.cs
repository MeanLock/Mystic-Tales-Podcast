using Architecture_1.DataAccess.Data;
using Architecture_1.DataAccess.Entities;

namespace Architecture_1.DataAccess.Repositories.interfaces
{
    public interface IFacilityRepository
    {
        Task<Facility> FindByIdAsync(int id);
        Task<bool> Deactivate(int id, bool deactivate);

    }
}
