using Architecture_1.DataAccess.Data;
using Architecture_1.DataAccess.Entities;

namespace Architecture_1.DataAccess.Repositories.interfaces
{
    public interface IServiceRepository
    {
        Task<Service> FindByIdAsync(int id);
        Task<IEnumerable<Service>> FindByFacilityMajorId(int facilityMajorId);
        Task<IEnumerable<Service>> FindAllAsync();
        Task<bool> Deactivate(int id, bool deactivate);
    }
}
