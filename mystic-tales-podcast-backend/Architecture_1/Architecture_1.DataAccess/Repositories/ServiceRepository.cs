using Microsoft.EntityFrameworkCore;
using Architecture_1.DataAccess.Data;
using Architecture_1.DataAccess.Entities;
using Architecture_1.DataAccess.Repositories.interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Architecture_1.DataAccess.Repositories
{
    public class ServiceRepository : IServiceRepository
    {
        private readonly AppDbContext _dbContext;

        public ServiceRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Service> FindByIdAsync(int id)
        {
            return await _dbContext.Services
                .Include(s => s.FacilityMajor)
                .Include(s => s.FacilityMajor.Facility)
                .Include(s => s.FacilityMajor.FacilityMajorType)
                .Include(s => s.ServiceType)
                .Include(s => s.ServiceRequests)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<IEnumerable<Service>> FindByFacilityMajorId(int facilityMajorId)
        {
            return await _dbContext.Services
                .Include(s => s.FacilityMajor)
                .Include(s => s.FacilityMajor.Facility)
                .Include(s => s.FacilityMajor.FacilityMajorType)
                .Include(s => s.ServiceType)
                .Where(s => s.FacilityMajorId == facilityMajorId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Service>> FindAllAsync()
        {
            return await _dbContext.Services
                .Include(s => s.FacilityMajor)
                .Include(s => s.FacilityMajor.Facility)
                .Include(s => s.FacilityMajor.FacilityMajorType)
                .Include(s => s.ServiceType)
                .Include(s => s.ServiceRequests)
                .ToListAsync();
        }

        public async Task<bool> Deactivate(int id, bool deactivate)
        {
            var item = await _dbContext.Services.FindAsync(id);
            if (item == null)
            {
                throw new Exception("Service không tồn tại");
            }
            item.IsDeactivated = deactivate;
            _dbContext.Entry(item).State = EntityState.Modified;

            var rowsAffected = await _dbContext.SaveChangesAsync();
            return rowsAffected > 1;
        }
    }
}
