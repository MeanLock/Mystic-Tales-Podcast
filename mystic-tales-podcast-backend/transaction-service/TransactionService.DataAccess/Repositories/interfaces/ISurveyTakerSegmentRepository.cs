using TransactionService.DataAccess.Data;
using TransactionService.DataAccess.Entities;

namespace TransactionService.DataAccess.Repositories.interfaces
{
    public interface ISurveyTakerSegmentRepository
    {
        Task UpdateAsync(int surveyId, SurveyTakerSegment surveyTakerSegment);
        
    }
}
