using Architecture_1.DataAccess.Data;
using Architecture_1.DataAccess.Entities;

namespace Architecture_1.DataAccess.Repositories.interfaces
{
    public interface IServiceRequestRepository
    {
        Task<ServiceRequest> FindByIdAsync(int id);
        Task<IEnumerable<ServiceRequest>> FindAllAsync();
        Task<IEnumerable<ServiceRequest>> FindByFacilityMajorId(int facilityMajorId);
        Task<IEnumerable<ServiceRequest>> FindByAssignedAssigneeId(int assigneeId);
        Task<IEnumerable<ServiceRequest>> FindByRequesterId(int requesterId);
        Task<IEnumerable<ServiceRequest>> FindByAssignedAssigneeIdAndFacilityMajorId(int assigneeId, int facilityMajorId);
        Task<int> CountByMajorIdFromThisMonth(int majorId);

        Task<IEnumerable<ServiceRequest>> FindByServiceTypeId(int serviceTypeId);

    }
}
