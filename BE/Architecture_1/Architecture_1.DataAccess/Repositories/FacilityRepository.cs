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
    public class FacilityRepository : IFacilityRepository
    {
        private readonly AppDbContext _dbContext;

        public FacilityRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Facility> FindByIdAsync(int id){
            return await _dbContext.Facilities
            .Include(f => f.FacilityMajors)
            .ThenInclude(fm => fm.TaskRequests)
            .Include(f => f.FacilityMajors)
            .ThenInclude(fm => fm.Services)
            .ThenInclude(s => s.ServiceRequests)
            .FirstOrDefaultAsync(f => f.Id == id);
        }

        public async Task<bool> Deactivate(int id, bool deactivate)
        {
            var item = await _dbContext.Facilities.FindAsync(id);
            if (item == null)
            {
                throw new Exception("Facility không tồn tại");
            }
            item.IsDeactivated = deactivate;
            _dbContext.Entry(item).State = EntityState.Modified;

            var rowsAffected = await _dbContext.SaveChangesAsync();
            return rowsAffected > 1;
        }
    }
}
