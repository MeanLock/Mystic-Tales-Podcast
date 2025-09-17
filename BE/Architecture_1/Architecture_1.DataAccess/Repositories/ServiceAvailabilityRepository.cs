using Architecture_1.DataAccess.Data;
using Architecture_1.DataAccess.Entities;
using Architecture_1.DataAccess.Repositories.interfaces;
using Microsoft.EntityFrameworkCore;

namespace Architecture_1.DataAccess.Repositories
{
    public class ServiceAvailabilityRepository : IServiceAvailabilityRepository
    {
        private readonly AppDbContext _dbContext;

        public ServiceAvailabilityRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<ServiceAvailability> FindByAvailability(ServiceAvailability serviceAvailability){
            return await _dbContext.ServiceAvailabilities
            .Include(sa => sa.Service)
            .Include(sa => sa.Service.FacilityMajor)
            .Include(sa => sa.Service.FacilityMajor.Facility)
            .Include(sa => sa.Service.FacilityMajor.FacilityMajorType)
            .FirstOrDefaultAsync(sa => sa.ServiceId == serviceAvailability.ServiceId && sa.DayOfWeek == serviceAvailability.DayOfWeek && sa.StartRequestableTime == serviceAvailability.StartRequestableTime);
        }

        public async Task<IEnumerable<ServiceAvailability>> FindByServiceId(int serviceId){
            return await _dbContext.ServiceAvailabilities
            .Include(sa => sa.Service)
            .Include(sa => sa.Service.FacilityMajor)
            .Include(sa => sa.Service.FacilityMajor.Facility)
            .Include(sa => sa.Service.FacilityMajor.FacilityMajorType)
            .Where(sa => sa.ServiceId == serviceId)
            .OrderBy(sa => sa.DayOfWeek) // Sắp xếp theo thứ tự ngày trong tuần (T2 -> CN)
            .ThenBy(sa => sa.StartRequestableTime) // Trong mỗi ngày, sắp xếp theo giờ bắt đầu
            .ToListAsync();
        }

        public async Task DeleteAsync(ServiceAvailability serviceAvailability){
            _dbContext.ServiceAvailabilities.Remove(serviceAvailability);
            await _dbContext.SaveChangesAsync();
        }
    }
}
