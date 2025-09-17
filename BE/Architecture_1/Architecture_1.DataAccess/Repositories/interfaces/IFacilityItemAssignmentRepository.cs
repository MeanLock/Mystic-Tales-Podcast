using Architecture_1.DataAccess.Data;
using Architecture_1.DataAccess.Entities;

namespace Architecture_1.DataAccess.Repositories.interfaces
{
    public interface IFacilityItemAssignmentRepository
    {
        Task<IEnumerable<FacilityItemAssignment>> FindByFacilityItemId(int facilityItemId);
        Task<int> GetInUseItemCount(int facilityItemId);
        Task<IEnumerable<FacilityItemAssignment>> FindByFacilityItemIdAndFacilityMajorId(int facilityItemId, int facilityMajorId);

        Task UpdateAsync(FacilityItemAssignment newFacilityItemAssignment);
        Task DeleteRangeAsync(IEnumerable<FacilityItemAssignment> facilityItemAssignments);
    }
}
