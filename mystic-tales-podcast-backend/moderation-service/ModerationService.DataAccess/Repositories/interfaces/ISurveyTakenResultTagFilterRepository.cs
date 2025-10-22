using ModerationService.DataAccess.Data;
using ModerationService.DataAccess.Entities;

namespace ModerationService.DataAccess.Repositories.interfaces
{
    public interface ISurveyTakenResultTagFilterRepository
    {
        Task<IEnumerable<SurveyTakenResult>> FindLimitByTakerIdAsync(int takerId, int limit);
    }
}
