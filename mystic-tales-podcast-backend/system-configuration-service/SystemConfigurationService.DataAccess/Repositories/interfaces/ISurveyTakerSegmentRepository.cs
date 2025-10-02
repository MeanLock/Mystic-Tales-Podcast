using SystemConfigurationService.DataAccess.Data;
using SystemConfigurationService.DataAccess.Entities;

namespace SystemConfigurationService.DataAccess.Repositories.interfaces
{
    public interface ISurveyTakerSegmentRepository
    {
        Task UpdateAsync(int surveyId, SurveyTakerSegment surveyTakerSegment);
        
    }
}
