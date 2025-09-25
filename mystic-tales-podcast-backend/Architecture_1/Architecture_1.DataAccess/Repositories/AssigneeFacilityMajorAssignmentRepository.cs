using Architecture_1.DataAccess.Data;
using Architecture_1.DataAccess.Entities;
using Architecture_1.DataAccess.Repositories.interfaces;
using Microsoft.EntityFrameworkCore;

namespace Architecture_1.DataAccess.Repositories
{
    public class AssigneeFacilityMajorAssignmentRepository : IAssigneeFacilityMajorAssignmentRepository
    {
        private readonly AppDbContext _dbContext;

        public AssigneeFacilityMajorAssignmentRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IEnumerable<AssigneeFacilityMajorAssignment>> FindByAccountId(int accountId){
            return await _dbContext.AssigneeFacilityMajorAssignments
            .Include(afma => afma.FacilityMajor)
            .Include(afma => afma.FacilityMajor.FacilityMajorType)
            .Include(afma => afma.FacilityMajor.Facility)
            .Include(afma => afma.Account)
            .Include(afma => afma.Account.Role)
            .Include(afma => afma.Account.JobType)
            .Where(afma => afma.AccountId == accountId)
            .ToListAsync();
        }

        public async Task<IEnumerable<AssigneeFacilityMajorAssignment>> FindByFacilityMajorId(int facilityMajorId){
            return await _dbContext.AssigneeFacilityMajorAssignments
            .Include(afma => afma.FacilityMajor)
            .Include(afma => afma.FacilityMajor.FacilityMajorType)
            .Include(afma => afma.FacilityMajor.Facility)
            .Include(afma => afma.Account)
            .Include(afma => afma.Account.Role)
            .Include(afma => afma.Account.JobType)
            .Where(afma => afma.FacilityMajorId == facilityMajorId)
            .ToListAsync();
        }

        public async Task<AssigneeFacilityMajorAssignment> FindByFacilityMajorIdAndAccountId(int facilityMajorId, int accountId){
            return await _dbContext.AssigneeFacilityMajorAssignments
            .Include(afma => afma.FacilityMajor)
            .Include(afma => afma.FacilityMajor.FacilityMajorType)
            .Include(afma => afma.FacilityMajor.Facility)
            .Include(afma => afma.Account)
            .Include(afma => afma.Account.Role)
            .Include(afma => afma.Account.JobType)
            .FirstOrDefaultAsync(afma => afma.FacilityMajorId == facilityMajorId && afma.AccountId == accountId);

        }

        public async Task<IEnumerable<AssigneeFacilityMajorAssignment>> FindAssignableAssigneeByMajorId(int facilityMajorId){
            return await _dbContext.AssigneeFacilityMajorAssignments
            .Include(afma => afma.FacilityMajor)
            .Include(afma => afma.FacilityMajor.FacilityMajorType)
            .Include(afma => afma.FacilityMajor.Facility)
            .Include(afma => afma.Account)
            .Include(afma => afma.Account.Role)
            .Include(afma => afma.Account.JobType)
            .Where(afma => afma.FacilityMajorId == facilityMajorId && afma.IsHead == false && afma.Account.IsDeactivated == false && afma.WorkDescription != null) 
            .ToListAsync();
        }

        public async Task UpdateAsync(AssigneeFacilityMajorAssignment newAssigneeFacilityMajorAssignment){
            var existAssigneeFacilityMajorAssignment = await _dbContext.AssigneeFacilityMajorAssignments.FirstOrDefaultAsync(afma => afma.FacilityMajorId == newAssigneeFacilityMajorAssignment.FacilityMajorId && afma.AccountId == newAssigneeFacilityMajorAssignment.AccountId);
            if(existAssigneeFacilityMajorAssignment == null){
                throw new Exception("AssigneeFacilityMajorAssignment không tồn tại");
            }
            existAssigneeFacilityMajorAssignment.WorkDescription = newAssigneeFacilityMajorAssignment.WorkDescription;
            _dbContext.Entry(existAssigneeFacilityMajorAssignment).State = EntityState.Modified;
            await _dbContext.SaveChangesAsync();
        }
        public async Task DeleteByAccountId(int accountId){
            var assigneeFacilityMajorAssignments = await _dbContext.AssigneeFacilityMajorAssignments.Where(afma => afma.AccountId == accountId).ToListAsync();
            _dbContext.AssigneeFacilityMajorAssignments.RemoveRange(assigneeFacilityMajorAssignments);
            await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteAsync(AssigneeFacilityMajorAssignment assigneeFacilityMajorAssignment){
            _dbContext.AssigneeFacilityMajorAssignments.Remove(assigneeFacilityMajorAssignment);
            await _dbContext.SaveChangesAsync();
        }
    }
}
