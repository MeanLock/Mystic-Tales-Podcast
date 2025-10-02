using TransactionService.DataAccess.Data;
using TransactionService.DataAccess.Entities;

namespace TransactionService.DataAccess.Repositories.interfaces
{
    public interface ISurveyTagFilterRepository
    {
        Task<IEnumerable<SurveyTagFilter>> FindBySurveyIdAsync(int surveyId);
        Task UpdateAsync(SurveyTagFilter surveyTagFilter);
        
    }
}
