using SurveyTalkService.DataAccess.Data;
using SurveyTalkService.DataAccess.Repositories.interfaces;
using Microsoft.EntityFrameworkCore;
using SurveyTalkService.DataAccess.Entities;

namespace SurveyTalkService.DataAccess.Repositories
{
    public class SurveyTagFilterRepository : ISurveyTagFilterRepository
    {
        private readonly AppDbContext _appDbContext;

        public SurveyTagFilterRepository(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        public async Task<IEnumerable<SurveyTagFilter>> FindBySurveyIdAsync(int surveyId)
        {
            return await _appDbContext.SurveyTagFilters
                .Include(surveyTagFilter => surveyTagFilter.FilterTag)
                .Where(surveyTagFilter => surveyTagFilter.SurveyId == surveyId)
                .ToListAsync();
        }

        public async Task UpdateAsync(SurveyTagFilter surveyTagFilter)
        {
            _appDbContext.SurveyTagFilters.Update(surveyTagFilter);
            await _appDbContext.SaveChangesAsync();
        }
    }
}
