using Architecture_1.DataAccess.Data;
using Architecture_1.DataAccess.Entities;

namespace Architecture_1.DataAccess.Repositories.interfaces
{
    public interface IAccountRepository
    {
        Task<IEnumerable<Account>> findByRoleId(int roleId);
        Task<IEnumerable<Account>> findByRoleIds(List<int> roleIds);

        Task<Account> FindByEmail(string email);

        Task<Account> FindById(int id);
        Task<bool> Deactivate(int id, bool deactivate);
    }
}
