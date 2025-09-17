using Architecture_1.DataAccess.Data;
using Architecture_1.DataAccess.Entities;

namespace Architecture_1.DataAccess.Repositories.interfaces
{
    public interface ITaskRequestRepository
    {
        Task<TaskRequest> FindByIdAsync(int id);
        Task<IEnumerable<TaskRequest>> FindByMajorHeadId(int majorHeadId);
        Task<IEnumerable<TaskRequest>> FindAllAsync();
        Task<IEnumerable<TaskRequest>> FindByFacilityMajorId(int facilityMajorId);

    }
}
