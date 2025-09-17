using Architecture_1.DataAccess.Data;
using Architecture_1.DataAccess.Entities;
using Architecture_1.DataAccess.Repositories.interfaces;
using Microsoft.EntityFrameworkCore;

namespace Architecture_1.DataAccess.Repositories
{
    public class FacilityMajorRepository : IFacilityMajorRepository
    {
        private readonly AppDbContext _dbContext;

        public FacilityMajorRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<FacilityMajor> FindByIdAsync(int id){
            return await _dbContext.FacilityMajors
            .Include(fm => fm.FacilityMajorType)
            .Include(fm => fm.Facility)
            .Include(fm => fm.Services)
            .ThenInclude(s => s.ServiceRequests)
            .Include(fm => fm.TaskRequests)
            .Include(fm => fm.FacilityItemAssignments)
            .ThenInclude(fia => fia.FacilityItem)
            .FirstOrDefaultAsync(fm => fm.Id == id);
        }

        public async Task<IEnumerable<FacilityMajor>> FindByFacilityId(int facilityId){
            return await _dbContext.FacilityMajors
            .Include(fm => fm.FacilityMajorType)
            .Include(fm => fm.Facility)
            .Where(fm => fm.FacilityId == facilityId).ToListAsync();
        }

        public async Task<IEnumerable<FacilityMajor>> FindByAllAsync()
        {
            return await _dbContext.FacilityMajors
            .Include(fm => fm.FacilityMajorType)
            .Include(fm => fm.Facility)
            .ToListAsync();
        }

        public async Task<bool> Deactivate(int id, bool deactivate)
        {
            var item = await _dbContext.FacilityMajors.FindAsync(id);
            if (item == null)
            {
                throw new Exception("Facility Major không tồn tại");
            }
            item.IsDeactivated = deactivate;
            _dbContext.Entry(item).State = EntityState.Modified;

            var rowsAffected = await _dbContext.SaveChangesAsync();
            return rowsAffected > 1;
        }
    }
}
