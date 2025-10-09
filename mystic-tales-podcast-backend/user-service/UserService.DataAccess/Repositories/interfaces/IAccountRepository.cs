using System.Linq.Expressions;
using UserService.DataAccess.Entities.SqlServer;

namespace UserService.DataAccess.Repositories.interfaces
{
    public interface IAccountRepository
    {
        Task<Account> FindByEmailAsync(string email, params Expression<Func<Account, object>>[] includeProperties);

        Task<IEnumerable<Account>> FindByRoleIdAsync(int roleId, Expression<Func<Account, bool>>? predicate = null, params Expression<Func<Account, object>>[] includeProperties);
        Task<IEnumerable<Account>> FindByRoleIdsAsync(List<int> roleIds, Expression<Func<Account, bool>>? predicate = null, params Expression<Func<Account, object>>[] includeProperties);
        // Task<bool> DeactivateAsync(int id, bool deactivate);
        // Task<int> CountAccountRegistrationByPeriodAsync(DateOnly startDate, DateOnly endDate);
    }
}
