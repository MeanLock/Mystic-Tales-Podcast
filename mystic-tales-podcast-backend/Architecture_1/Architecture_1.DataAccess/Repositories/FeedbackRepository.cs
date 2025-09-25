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
    internal class FeedbackRepository : IFeedbackRepository
    {
        private readonly AppDbContext _dbContext;

        public FeedbackRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IEnumerable<Feedback>> FindByMajorId(int majorId)
        {
            return await _dbContext.Feedbacks
            .Include(f => f.Account)
            .Include(f => f.FacilityMajor)
            .Include(f => f.FacilityMajor.Facility)
            .Include(f => f.FacilityMajor.FacilityMajorType)
            .Where(f => f.FacilityMajorId == majorId)
            .ToListAsync();
        }

        public async Task<IEnumerable<Feedback>> FindAllAsync()
        {
            return await _dbContext.Feedbacks
            .Include(f => f.Account)
            .Include(f => f.FacilityMajor)
            .ToListAsync();
        }



        public async Task<bool> Deactivate(int id, bool deactivate)
        {
            var item = await _dbContext.Feedbacks.FindAsync(id);
            if (item == null)
            {
                throw new Exception("Feedback không tồn tại");
            }
            item.IsDeactivated = deactivate;
            _dbContext.Entry(item).State = EntityState.Modified;

            var rowsAffected = await _dbContext.SaveChangesAsync();
            return rowsAffected > 1;
        }
    }
}
