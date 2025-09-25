using SurveyTalkService.DataAccess.Data;
using SurveyTalkService.DataAccess.Repositories.interfaces;
using Microsoft.EntityFrameworkCore;
using SurveyTalkService.DataAccess.Entities;

namespace SurveyTalkService.DataAccess.Repositories
{
    public class SurveyTakenResultTagFilterRepository : ISurveyTakenResultTagFilterRepository
    {
        private readonly AppDbContext _appDbContext;

        public SurveyTakenResultTagFilterRepository(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        public async Task<IEnumerable<SurveyTakenResult>> FindLimitByTakerIdAsync(int takerId, int limit)
        {
            return await _appDbContext.SurveyTakenResultTagFilters
                .Include(strtf => strtf.SurveyTakenResult)
                .Include(strtf => strtf.AdditionalFilterTag)
                .Where(strtf => strtf.SurveyTakenResult.TakerId == takerId)
                .Select(strtf => strtf.SurveyTakenResult)
                .Distinct()
                .OrderByDescending(str => str.CompletedAt)
                .Take(limit)
                .ToListAsync();
        }

        
    }
}
