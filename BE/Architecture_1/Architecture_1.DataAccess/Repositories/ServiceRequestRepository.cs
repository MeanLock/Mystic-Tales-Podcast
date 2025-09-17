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
    internal class ServiceRequestRepository : IServiceRequestRepository
    {
        private readonly AppDbContext _dbContext;

        public ServiceRequestRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<ServiceRequest> FindByIdAsync(int id)
        {
            return await _dbContext.ServiceRequests
            .Include(f => f.Requester)
            .Include(f => f.AssignedAssignee)
            .Include(f => f.Service)
            .Include(f => f.Service.FacilityMajor)
            .Include(f => f.Service.FacilityMajor.Facility)
            .Include(f => f.Service.FacilityMajor.FacilityMajorType)
            .Include(f => f.RequestStatus)
            .FirstOrDefaultAsync(f => f.Id == id);
        }
        public async Task<IEnumerable<ServiceRequest>> FindAllAsync()
        {
            return await _dbContext.ServiceRequests
            .Include(f => f.Requester)
            .Include(f => f.AssignedAssignee)
            .Include(f => f.Service)
            .Include(f => f.Service.FacilityMajor)
            .Include(f => f.Service.FacilityMajor.Facility)
            .Include(f => f.Service.FacilityMajor.FacilityMajorType)
            .Include(f => f.RequestStatus)
            .OrderByDescending(f => f.UpdatedAt)
            .ToListAsync();
        }

        public async Task<IEnumerable<ServiceRequest>> FindByFacilityMajorId(int facilityMajorId)
        {
            return await _dbContext.ServiceRequests
            .Include(f => f.Requester)
            .Include(f => f.Requester.Role)
            .Include(f => f.Requester.JobType)
            .Include(f => f.AssignedAssignee)
            .Include(f => f.Service)
            .Include(f => f.Service.FacilityMajor)
            .Include(f => f.Service.FacilityMajor.Facility)
            .Include(f => f.Service.FacilityMajor.FacilityMajorType)
            .Include(f => f.RequestStatus)
            .Where(f => f.Service.FacilityMajorId == facilityMajorId)
            .OrderByDescending(f => f.UpdatedAt)
            .ToListAsync();
        }

        public async Task<IEnumerable<ServiceRequest>> FindByAssignedAssigneeId(int assigneeId)
        {
            return await _dbContext.ServiceRequests
            .Include(f => f.Requester)
            .Include(f => f.AssignedAssignee)
            .Include(f => f.Service)
            .Include(f => f.Service.FacilityMajor)
            .Include(f => f.Service.FacilityMajor.Facility)
            .Include(f => f.Service.FacilityMajor.FacilityMajorType)
            .Include(f => f.RequestStatus)
            .Where(f => f.AssignedAssigneeId == assigneeId)
            .OrderByDescending(f => f.UpdatedAt)
            .ToListAsync();
        }

        public async Task<IEnumerable<ServiceRequest>> FindByRequesterId(int requesterId){
            return await _dbContext.ServiceRequests
            .Include(f => f.Requester)
            .Include(f => f.AssignedAssignee)
            .Include(f => f.Service)
            .Include(f => f.Service.FacilityMajor)
            .Include(f => f.Service.FacilityMajor.Facility)
            .Include(f => f.Service.FacilityMajor.FacilityMajorType)
            .Include(f => f.RequestStatus)
            .Where(f => f.RequesterId == requesterId)
            .OrderByDescending(f => f.UpdatedAt)
            .ToListAsync();
        }

        public async Task<IEnumerable<ServiceRequest>> FindByAssignedAssigneeIdAndFacilityMajorId(int assigneeId, int facilityMajorId)
        {
            return await _dbContext.ServiceRequests
            .Include(f => f.Requester)
            .Include(f => f.AssignedAssignee)
            .Include(f => f.Service)
            .Include(f => f.Service.FacilityMajor)
            .Include(f => f.Service.FacilityMajor.Facility)
            .Include(f => f.Service.FacilityMajor.FacilityMajorType)
            .Include(f => f.RequestStatus)
            .Where(f => f.AssignedAssigneeId == assigneeId && f.Service.FacilityMajorId == facilityMajorId)
            .OrderByDescending(f => f.UpdatedAt)
            .ToListAsync();
        }

        public async Task<int> CountByMajorIdFromThisMonth(int majorId){
            return await _dbContext.ServiceRequests
            .Include(f => f.Requester)
            .Include(f => f.AssignedAssignee)
            .Include(f => f.Service)
            .Include(f => f.Service.FacilityMajor)
            .Include(f => f.Service.FacilityMajor.Facility)
            .Include(f => f.Service.FacilityMajor.FacilityMajorType)
            .Include(f => f.RequestStatus)
            .Where(f => f.Service.FacilityMajorId == majorId && f.CreatedAt.Value.Month == DateTime.Now.Month && f.CreatedAt.Value.Year == DateTime.Now.Year)
            .CountAsync();
        }

        public async Task<IEnumerable<ServiceRequest>> FindByServiceTypeId(int serviceTypeId)
        {
            return await _dbContext.ServiceRequests
            .Include(f => f.Requester)
            .Include(f => f.AssignedAssignee)
            .Include(f => f.Service)
            .Include(f => f.Service.FacilityMajor)
            .Include(f => f.Service.FacilityMajor.Facility)
            .Include(f => f.Service.FacilityMajor.FacilityMajorType)
            .Include(f => f.RequestStatus)
            .Where(f => f.Service.ServiceTypeId == serviceTypeId)
            .OrderByDescending(f => f.UpdatedAt)
            .ToListAsync();
        }



    }
}
