using ModerationService.DataAccess.Data;
using ModerationService.DataAccess.Entities;

namespace ModerationService.DataAccess.Repositories.interfaces
{
    public interface ISurveyTakerSegmentRepository
    {
        Task UpdateAsync(int surveyId, SurveyTakerSegment surveyTakerSegment);
        
    }
}
