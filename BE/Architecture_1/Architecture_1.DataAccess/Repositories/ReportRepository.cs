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
    internal class ReportRepository : IReportRepository
    {
        private readonly AppDbContext _dbContext;

        public ReportRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IEnumerable<Report>> FindByMajorId(int majorId)
        {
            return await _dbContext.Reports
            .Include(f => f.Account)
            .Include(f => f.ReportType)
            .Include(f => f.FacilityMajor)
            .Include(f => f.FacilityMajor.Facility)
            .Include(f => f.FacilityMajor.FacilityMajorType)
            .Where(f => f.FacilityMajorId == majorId)
            .ToListAsync();
        }

        public async Task<Report> FindByIdAsync(int id)
        {
            return await _dbContext.Reports
            .Include(f => f.Account)
            .Include(f => f.ReportType)
            .Include(f => f.FacilityMajor)
            .Include(f => f.FacilityMajor.Facility)
            .Include(f => f.FacilityMajor.FacilityMajorType)
            .FirstOrDefaultAsync(f => f.Id == id);
        }

        public async Task<IEnumerable<Report>> FindAllAsync()
        {
            return await _dbContext.Reports
            .Include(f => f.Account)
            .Include(f => f.ReportType)
            .Include(f => f.FacilityMajor)
            .Include(f => f.FacilityMajor.Facility)
            .Include(f => f.FacilityMajor.FacilityMajorType)
            .ToListAsync();

        }



        public async Task<bool> Resolve(int id, bool IsResolved)
        {
            var item = await _dbContext.Reports.FindAsync(id);
            if (item == null)
            {
                throw new Exception("Report không tồn tại");
            }
            item.IsResolved = IsResolved;
            _dbContext.Entry(item).State = EntityState.Modified;

            var rowsAffected = await _dbContext.SaveChangesAsync();
            return rowsAffected > 1;
        }
    }
}
