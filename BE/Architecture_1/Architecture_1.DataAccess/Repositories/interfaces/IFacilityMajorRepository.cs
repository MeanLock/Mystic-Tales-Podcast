using Architecture_1.DataAccess.Data;
using Architecture_1.DataAccess.Entities;

namespace Architecture_1.DataAccess.Repositories.interfaces
{
    public interface IFacilityMajorRepository
    {
        Task<FacilityMajor> FindByIdAsync(int id);
        Task<IEnumerable<FacilityMajor>> FindByFacilityId(int facilityId);
        Task<IEnumerable<FacilityMajor>> FindByAllAsync();
        Task<bool> Deactivate(int id, bool deactivate);
    }
}
