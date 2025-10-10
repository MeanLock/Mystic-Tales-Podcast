using UserService.DataAccess.Data;
using UserService.DataAccess.Repositories.interfaces;
using Microsoft.EntityFrameworkCore;
using UserService.DataAccess.Entities;
using UserService.Common.AppConfigurations.App.interfaces;
using UserService.DataAccess.Entities.SqlServer;
using System.Linq.Expressions;

namespace UserService.DataAccess.Repositories
{
    public class AccountRepository : IAccountRepository
    {
        private readonly AppDbContext _appDbContext;
        private readonly IAppConfig _appConfig;

        public AccountRepository(AppDbContext appDbContext, IAppConfig appConfig)
        {
            _appConfig = appConfig;

            _appDbContext = appDbContext;
        }


        public async Task<Account> FindByEmailAsync(string email, Func<IQueryable<Account>, IQueryable<Account>>? includeFunc = null)
        {
            IQueryable<Account> query = _appDbContext.Accounts;

            if (includeFunc != null)
            {
                query = includeFunc(query);
            }

            return await query.FirstOrDefaultAsync(account => account.Email == email);
        }



        public async Task<IEnumerable<Account>> FindByRoleIdAsync(int roleId, Expression<Func<Account, bool>>? predicate = null, Func<IQueryable<Account>, IQueryable<Account>>? includeFunc = null)
        {
            IQueryable<Account> query = _appDbContext.Accounts;

            if (includeFunc != null)
            {
                query = includeFunc(query);
            }

            if (predicate != null)
            {
                query = query.Where(predicate);
            }

            return await query
                .Where(account => account.RoleId == roleId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Account>> FindByRoleIdsAsync(List<int> roleIds, Expression<Func<Account, bool>>? predicate = null, Func<IQueryable<Account>, IQueryable<Account>>? includeFunc = null)
        {
            IQueryable<Account> query = _appDbContext.Accounts;

            if (includeFunc != null)
            {
                query = includeFunc(query);
            }

            if (predicate != null)
            {
                query = query.Where(predicate);
            }

            return await query
                .Where(account => roleIds.Contains(account.RoleId))
                .ToListAsync();
        }

        // public async Task<IEnumerable<Account>> FindByRoleIdsAsync(List<int> roleIds)
        // {
        //     return await _appDbContext.Accounts
        //         .Include(account => account.Role)
        //         .Include(account => account.AccountProfile)
        //         .Include(account => account.PlatformFeedback)
        //         .Where(account => account.RoleId == roleId)
        //         .ToListAsync();
        // }



        // public async Task<Account> FindByIdAsync(int id)
        // {
        //     return await _appDbContext.Accounts
        //         .Include(account => account.Role)
        //         .Include(account => account.AccountProfile)
        //         .Include(account => account.PlatformFeedback)
        //         .Where(account => roleIds.Contains(account.RoleId))
        //         .ToListAsync();
        // }

        // public async Task<Account> FindByIdAsync(int id)
        // {
        //     return await _appDbContext.Accounts
        //         .Include(account => account.Role)
        //         .Include(account => account.AccountProfile)
        //         .Include(account => account.AccountNationalVerification)
        //         .Include(account => account.TakerTagFilters)
        //         .ThenInclude(takerTagFilter => takerTagFilter.FilterTag)
        //         .Include(account => account.SurveyTopicFavorites)
        //         .Include(account => account.PlatformFeedback)
        //         .FirstOrDefaultAsync(account => account.Id == id);
        // }


        // public async Task<bool> DeactivateAsync(int id, bool deactivate)
        // {
        //     var account = await _appDbContext.Accounts.FindAsync(id);
        //     if (account == null)
        //     {
        //         throw new Exception("Account không tồn tại");
        //     }
        //     if (deactivate)
        //     {
        //         var tz = TimeZoneInfo.FindSystemTimeZoneById(_appConfig.TIME_ZONE);
        //         account.DeactivatedAt = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz);
        //     }
        //     else
        //     {
        //         account.DeactivatedAt = null;
        //     }

        //     _appDbContext.Entry(account).State = EntityState.Modified;

        //     var rowsAffected = await _appDbContext.SaveChangesAsync();
        //     return rowsAffected > 1;
        // }

        // public async Task<int> CountAccountRegistrationByPeriodAsync(DateOnly startDate, DateOnly endDate)
        // {
        //     return await _appDbContext.Accounts
        //         .Where(account => DateOnly.FromDateTime(account.CreatedAt.Date) >= startDate &&
        //                           DateOnly.FromDateTime(account.CreatedAt.Date) <= endDate)
        //         .CountAsync();
        // }


    }
}
