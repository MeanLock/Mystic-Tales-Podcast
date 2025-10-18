using TransactionService.DataAccess.Data;
using TransactionService.DataAccess.Repositories.interfaces;
using Microsoft.EntityFrameworkCore;
using TransactionService.DataAccess.Entities;

namespace TransactionService.DataAccess.Repositories
{
    public class SurveyTakerSegmentRepository : ISurveyTakerSegmentRepository
    {
        private readonly AppDbContext _appDbContext;

        public SurveyTakerSegmentRepository(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        public async Task UpdateAsync(int surveyId, SurveyTakerSegment surveyTakerSegment)
        {
            var existingSegment = await _appDbContext.SurveyTakerSegments
                .FirstOrDefaultAsync(s => s.SurveyId == surveyId);

            if (existingSegment != null)
            {
                _appDbContext.Entry(existingSegment).CurrentValues.SetValues(surveyTakerSegment);

                await _appDbContext.SaveChangesAsync();
            }
        }
    }
}
