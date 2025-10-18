using Architecture_1.DataAccess.Data;
using Architecture_1.DataAccess.Entities;

namespace Architecture_1.DataAccess.Repositories.interfaces
{
    public interface IServiceAvailabilityRepository
    {
        Task<ServiceAvailability> FindByAvailability(ServiceAvailability serviceAvailability);
        Task<IEnumerable<ServiceAvailability>> FindByServiceId(int serviceId);

        Task DeleteAsync(ServiceAvailability serviceAvailability);
    }
}
