using Architecture_1.DataAccess.Data;
using Architecture_1.DataAccess.Entities;
using Architecture_1.DataAccess.Repositories.interfaces;
using Microsoft.EntityFrameworkCore;

namespace Architecture_1.DataAccess.Repositories
{
    public class FacilityItemAssignmentRepository : IFacilityItemAssignmentRepository
    {
        private readonly AppDbContext _dbContext;

        public FacilityItemAssignmentRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IEnumerable<FacilityItemAssignment>> FindByFacilityItemId(int facilityItemId){
            return await _dbContext.FacilityItemAssignments
            .Include(fia => fia.FacilityItem)
            .Include(fia => fia.FacilityMajor)
            .Where(fia => fia.FacilityItemId == facilityItemId).ToListAsync();
        }
        public async Task<int> GetInUseItemCount(int facilityItemId){
            return (await _dbContext.FacilityItemAssignments.Where(fia => fia.FacilityItemId == facilityItemId).ToListAsync()).Sum(fia => fia.ItemCount);
        }
        public async Task<IEnumerable<FacilityItemAssignment>> FindByFacilityItemIdAndFacilityMajorId(int facilityItemId, int facilityMajorId){

            return await _dbContext.FacilityItemAssignments
            .Include(fia => fia.FacilityItem)
            .Include(fia => fia.FacilityMajor)
            .Where(fia => fia.FacilityItemId == facilityItemId && fia.FacilityMajorId == facilityMajorId).ToListAsync();
        }

        public async Task UpdateAsync(FacilityItemAssignment newFacilityItemAssignment){
            var existFacilityItemAssignment = await _dbContext.FacilityItemAssignments.FirstOrDefaultAsync(fia => fia.FacilityItemId == newFacilityItemAssignment.FacilityItemId && fia.FacilityMajorId == newFacilityItemAssignment.FacilityMajorId);
            if(existFacilityItemAssignment == null){
                throw new Exception("FacilityItemAssignment không tồn tại");
            }
            existFacilityItemAssignment.ItemCount = newFacilityItemAssignment.ItemCount;
            _dbContext.Entry(existFacilityItemAssignment).State = EntityState.Modified;
            await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteRangeAsync(IEnumerable<FacilityItemAssignment> facilityItemAssignments){
            foreach(var facilityItemAssignment in facilityItemAssignments){
                _dbContext.Entry(facilityItemAssignment).State = EntityState.Deleted;
                _dbContext.FacilityItemAssignments.Remove(facilityItemAssignment);
            }
            // _dbContext.FacilityItemAssignments.RemoveRange(facilityItemAssignments);
            await _dbContext.SaveChangesAsync();
        }
    }
}
