using System.Linq.Expressions;
using UserService.DataAccess.Entities.SqlServer;

namespace UserService.DataAccess.Repositories.interfaces
{
    public interface IAccountRepository
    {
        Task<Account> FindByEmailAsync(string email, Func<IQueryable<Account>, IQueryable<Account>>? includeFunc = null);

        Task<IEnumerable<Account>> FindByRoleIdAsync(int roleId, Expression<Func<Account, bool>>? predicate = null, Func<IQueryable<Account>, IQueryable<Account>>? includeFunc = null);

        Task<IEnumerable<Account>> FindByRoleIdsAsync(List<int> roleIds, Expression<Func<Account, bool>>? predicate = null, Func<IQueryable<Account>, IQueryable<Account>>? includeFunc = null);
        // Task<bool> DeactivateAsync(int id, bool deactivate);
        // Task<int> CountAccountRegistrationByPeriodAsync(DateOnly startDate, DateOnly endDate);
    }
}
