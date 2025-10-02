using SurveyTalkService.DataAccess.Data;
using SurveyTalkService.DataAccess.Entities;

namespace SurveyTalkService.DataAccess.Repositories.interfaces
{
    public interface ITakerTagFilterRepository
    {
        Task UpdateAsync(TakerTagFilter takerTagFilter);
        
    }
}
