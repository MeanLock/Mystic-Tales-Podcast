using PodcastService.DataAccess.Data;
using PodcastService.DataAccess.Entities;

namespace PodcastService.DataAccess.Repositories.interfaces
{
    public interface ISurveyTakenResultTagFilterRepository
    {
        Task<IEnumerable<SurveyTakenResult>> FindLimitByTakerIdAsync(int takerId, int limit);
    }
}
