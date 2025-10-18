using SystemConfigurationService.DataAccess.Data;
using SystemConfigurationService.DataAccess.Entities;

namespace SystemConfigurationService.DataAccess.Repositories.interfaces
{
    public interface ISurveyTakenResultTagFilterRepository
    {
        Task<IEnumerable<SurveyTakenResult>> FindLimitByTakerIdAsync(int takerId, int limit);
    }
}
