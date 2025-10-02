using BookingManagementService.DataAccess.Data;
using BookingManagementService.DataAccess.Entities;

namespace BookingManagementService.DataAccess.Repositories.interfaces
{
    public interface ISurveyTakerSegmentRepository
    {
        Task UpdateAsync(int surveyId, SurveyTakerSegment surveyTakerSegment);
        
    }
}
