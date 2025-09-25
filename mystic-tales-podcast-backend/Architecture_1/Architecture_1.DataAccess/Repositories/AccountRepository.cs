using Architecture_1.DataAccess.Data;
using Architecture_1.DataAccess.Entities;
using Architecture_1.DataAccess.Repositories.interfaces;
using Microsoft.EntityFrameworkCore;

namespace Architecture_1.DataAccess.Repositories
{
    public class AccountRepository : IAccountRepository
    {
        private readonly AppDbContext _dbContext;

        public AccountRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IEnumerable<Account>> findByRoleId(int roleId)
        {
            return await _dbContext.Accounts
                .Include(account => account.Role)
                .Include(account => account.JobType)
                .Where(account => account.RoleId == roleId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Account>> findByRoleIds(List<int> roleIds)
        {
            return await _dbContext.Accounts
                .Include(account => account.Role)
                .Include(account => account.JobType)
                .Where(account => roleIds.Contains(account.RoleId))
                .ToListAsync();
        }

        public async Task<Account> FindByEmail(string email)
        {
            return await _dbContext.Accounts
                .Include(account => account.Role)
                .Include(account => account.JobType)
                .FirstOrDefaultAsync(account => account.Email == email);
        }

        public async Task<Account> FindById(int id)
        {
            return await _dbContext.Accounts
                .Include(account => account.Role)
                .Include(account => account.JobType)
                .Include(account => account.AssigneeFacilityMajorAssignments)
                .FirstOrDefaultAsync(account => account.Id == id);
        }

        public async Task<bool> Deactivate(int id, bool deactivate)
        {
            var account = await _dbContext.Accounts.FindAsync(id);
            if (account == null)
            {
                throw new Exception("Account không tồn tại");
            }
            account.IsDeactivated = deactivate;
            _dbContext.Entry(account).State = EntityState.Modified;

            var rowsAffected = await _dbContext.SaveChangesAsync();
            return rowsAffected > 1;
        }
    }
}
