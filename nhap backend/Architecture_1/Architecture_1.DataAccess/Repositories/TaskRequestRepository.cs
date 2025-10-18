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
    internal class TaskRequestRepository : ITaskRequestRepository
    {
        private readonly AppDbContext _dbContext;

        public TaskRequestRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<TaskRequest> FindByIdAsync(int id)
        {
            return await _dbContext.TaskRequests
            .Include(f => f.FacilityMajor)
            .Include(f => f.FacilityMajor.Facility)
            .Include(f => f.FacilityMajor.FacilityMajorType)
            .Include(f => f.RequestStatus)
            .FirstOrDefaultAsync(f => f.Id == id);
        }
        public async Task<IEnumerable<TaskRequest>> FindAllAsync()
        {
            return await _dbContext.TaskRequests
            .Include(f => f.FacilityMajor)
            .Include(f => f.FacilityMajor.Facility)
            .Include(f => f.FacilityMajor.FacilityMajorType)
            .Include(f => f.RequestStatus)
            .OrderByDescending(f => f.UpdatedAt)
            .ToListAsync();
        }

        public async Task<IEnumerable<TaskRequest>> FindByMajorHeadId(int majorHeadId)
        {
            var majors = await _dbContext.AssigneeFacilityMajorAssignments
            .Where(f => f.AccountId == majorHeadId && f.IsHead == true)
            .ToListAsync();

            var majorIds = majors.Select(f => f.FacilityMajorId).ToList();


            return await _dbContext.TaskRequests
            .Include(f => f.FacilityMajor)
            .Include(f => f.FacilityMajor.Facility)
            .Include(f => f.FacilityMajor.FacilityMajorType)
            .Include(f => f.RequestStatus)
            .Where(f => majorIds.Contains(f.FacilityMajorId))
            .OrderByDescending(f => f.UpdatedAt)
            .ToListAsync();
        }

        public async Task<IEnumerable<TaskRequest>> FindByFacilityMajorId(int facilityMajorId)
        {
            return await _dbContext.TaskRequests
            .Include(f => f.FacilityMajor)
            .Include(f => f.FacilityMajor.Facility)
            .Include(f => f.FacilityMajor.FacilityMajorType)
            .Include(f => f.RequestStatus)
            .Where(f => f.FacilityMajorId == facilityMajorId)
            .OrderByDescending(f => f.UpdatedAt)
            .ToListAsync();
        }



    }
}
