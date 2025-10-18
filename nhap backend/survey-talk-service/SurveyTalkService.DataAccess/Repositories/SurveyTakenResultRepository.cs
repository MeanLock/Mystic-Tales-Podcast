using SurveyTalkService.DataAccess.Data;
using SurveyTalkService.DataAccess.Repositories.interfaces;
using Microsoft.EntityFrameworkCore;
using SurveyTalkService.DataAccess.Entities;

namespace SurveyTalkService.DataAccess.Repositories
{
    public class SurveyTakenResultRepository : ISurveyTakenResultRepository
    {
        private readonly AppDbContext _appDbContext;

        public SurveyTakenResultRepository(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        public async Task<IEnumerable<SurveyTakenResult>> FindByAccountIdsAndDateAsync(List<int> accountIds, DateOnly date, bool? isInvalidTakenResultContain = true)
        {
            var query = _appDbContext.SurveyTakenResults
                .Include(str => str.Survey)
                .Include(str => str.SurveyResponses)
                .Where(str => accountIds.Contains(str.TakerId) && DateOnly.FromDateTime(str.CompletedAt) == date);

            if (isInvalidTakenResultContain == false)
            {
                query = query.Where(str => str.IsValid == true);
            }
            return await query.OrderByDescending(str => str.CompletedAt).ToListAsync();
        }

        public async Task<IEnumerable<SurveyTakenResult>> FindByAccountIdsAndDatePeriodAsync(List<int> accountIds, DateOnly startDate, DateOnly endDate, bool? isInvalidTakenResultContain = true)
        {
            var surveyTakenResults = await _appDbContext.SurveyTakenResults
                .Include(str => str.Survey)
                .Where(str => accountIds.Contains(str.TakerId) &&
                              DateOnly.FromDateTime(str.CompletedAt) >= startDate &&
                              DateOnly.FromDateTime(str.CompletedAt) <= endDate)
                .ToListAsync();

            if (isInvalidTakenResultContain == false)
            {
                surveyTakenResults = surveyTakenResults.Where(str => str.IsValid == true).ToList();
            }

            // return accountIds.Select(id => surveyTakenResults.Where(str => str.TakerId == id));
            return surveyTakenResults
                .GroupBy(str => str.TakerId)
                .Select(g => g.OrderByDescending(str => str.CompletedAt).FirstOrDefault())
                .OrderByDescending(str => str.CompletedAt);
        }

        public async Task<IEnumerable<SurveyTakenResult>> FindBySurveyIdAsync(int surveyId, bool? IsInvalidTakenResultContain = true)
        {
            var query = _appDbContext.SurveyTakenResults
                .Include(str => str.Taker)
                .Include(str => str.SurveyResponses)
                .ThenInclude(str => str.SurveyQuestion)
                .Where(str => str.SurveyId == surveyId);

            if (IsInvalidTakenResultContain == false)
            {
                query = query.Where(str => str.IsValid == true);
            }

            return await query.OrderByDescending(str => str.CompletedAt).ToListAsync();
        }

        public async Task<IEnumerable<SurveyTakenResult>> FindByTakerIdAsync(int takerId, int? limit = null)
        {
            var query = _appDbContext.SurveyTakenResults
                .Include(str => str.Survey)
                .Include(str => str.SurveyResponses)
                .Include(str => str.SurveyTakenResultTagFilters)
                    .ThenInclude(strtf => strtf.AdditionalFilterTag)
                .Where(str => str.TakerId == takerId)
                .OrderByDescending(str => str.CompletedAt);

            // Khai báo resultQuery là IQueryable để tránh lỗi ép kiểu
            IQueryable<SurveyTakenResult> resultQuery = query;
            if (limit.HasValue && limit.Value > 0)
            {
                resultQuery = resultQuery.Take(limit.Value);
            }

            return await resultQuery.ToListAsync();
        }

        public async Task<SurveyTakenResult> FindByTakerIdAndSurveyIdAsync(int takerId, int surveyId)
        {
            return await _appDbContext.SurveyTakenResults
                .Include(str => str.Survey)
                .Include(str => str.SurveyResponses)
                .Include(str => str.SurveyTakenResultTagFilters)
                .FirstOrDefaultAsync(str => str.TakerId == takerId && str.SurveyId == surveyId);
        }

        public async Task<int> CountBySurveyIdAsync(int surveyId, bool? isInvalidTakenResultContain = null)
        {
            var query = _appDbContext.SurveyTakenResults
                .Where(str => str.SurveyId == surveyId);

            if (isInvalidTakenResultContain == false)
            {
                query = query.Where(str => str.IsValid == true);
            }

            return await query.CountAsync();
        }
    }
}
