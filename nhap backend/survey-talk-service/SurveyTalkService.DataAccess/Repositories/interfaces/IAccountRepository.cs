using SurveyTalkService.DataAccess.Data;
using SurveyTalkService.DataAccess.Entities;

namespace SurveyTalkService.DataAccess.Repositories.interfaces
{
    public interface IAccountRepository
    {
        Task<IEnumerable<Account>> FindByRoleIdAsync(int roleId);
        Task<IEnumerable<Account>> FindByRoleIdsAsync(List<int> roleIds);

        Task<Account> FindByEmailAsync(string email);

        Task<Account> FindByIdAsync(int id);
        Task<IEnumerable<Account>> FindByAccountProfileAsync(AccountProfile accountProfile, bool? IsDeletedContain = false);
        bool CompareAccountProfile(AccountProfile accountProfile, AccountProfile targetAccountProfile);
        Task<bool> DeactivateAsync(int id, bool deactivate);
        Task<int> CountAccountRegistrationByPeriodAsync(DateOnly startDate, DateOnly endDate);
    }
}
