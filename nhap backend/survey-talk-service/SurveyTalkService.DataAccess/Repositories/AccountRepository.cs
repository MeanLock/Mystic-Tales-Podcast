using SurveyTalkService.DataAccess.Data;
using SurveyTalkService.DataAccess.Repositories.interfaces;
using Microsoft.EntityFrameworkCore;
using SurveyTalkService.DataAccess.Entities;
using SurveyTalkService.Common.AppConfigurations.App.interfaces;

namespace SurveyTalkService.DataAccess.Repositories
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

        public async Task<IEnumerable<Account>> FindByRoleIdAsync(int roleId)
        {
            return await _appDbContext.Accounts
                .Include(account => account.Role)
                .Include(account => account.AccountProfile)
                .Include(account => account.AccountNationalVerification)
                .Include(account => account.PlatformFeedback)
                .Where(account => account.RoleId == roleId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Account>> FindByRoleIdsAsync(List<int> roleIds)
        {
            return await _appDbContext.Accounts
                .Include(account => account.Role)
                .Include(account => account.AccountProfile)
                .Include(account => account.AccountNationalVerification)
                .Include(account => account.PlatformFeedback)
                .Where(account => roleIds.Contains(account.RoleId))
                .ToListAsync();
        }

        public async Task<Account> FindByEmailAsync(string email)
        {
            return await _appDbContext.Accounts
                .Include(account => account.Role)
                .Include(account => account.AccountProfile)
                .Include(account => account.AccountNationalVerification)
                .Include(account => account.PlatformFeedback)
                .FirstOrDefaultAsync(account => account.Email == email);
        }

        public async Task<Account> FindByIdAsync(int id)
        {
            return await _appDbContext.Accounts
                .Include(account => account.Role)
                .Include(account => account.AccountProfile)
                .Include(account => account.AccountNationalVerification)
                .Include(account => account.TakerTagFilters)
                .ThenInclude(takerTagFilter => takerTagFilter.FilterTag)
                .Include(account => account.SurveyTopicFavorites)
                .Include(account => account.PlatformFeedback)
                .FirstOrDefaultAsync(account => account.Id == id);
        }

        public async Task<IEnumerable<Account>> FindByAccountProfileAsync(AccountProfile accountProfile, bool? IsDeactivatedContain = false)
        {
            var accounts = await _appDbContext.Accounts
                .Include(account => account.AccountProfile)
                .Include(account => account.TakerTagFilters)
                .Where(account =>
                    (IsDeactivatedContain == true || (IsDeactivatedContain == false && account.DeactivatedAt == null))
                )
                .ToListAsync();

            var result = accounts.Where(account =>
                account.AccountProfile != null && CompareAccountProfile(accountProfile, account.AccountProfile)
            );

            return result;
        }

        public bool CompareAccountProfile(AccountProfile accountProfile, AccountProfile targetAccountProfile)
        {
            // so sánh contain sau khi split " | " ở cả hai AccountProfile nếu ở cột nào đó chỉ cần có phần tử trùng là true, và phải true ở tất cả các cột mới trả về true
            var countryRegions = accountProfile.CountryRegion?.Split(new[] { " | " }, StringSplitOptions.None).Select(x => x.Trim()).ToArray();
            var targetCountryRegions = targetAccountProfile.CountryRegion?.Split(new[] { " | " }, StringSplitOptions.None).Select(x => x.Trim()).ToArray();
            var maritalStatuses = accountProfile.MaritalStatus?.Split(new[] { " | " }, StringSplitOptions.None).Select(x => x.Trim()).ToArray();
            var targetMaritalStatuses = targetAccountProfile.MaritalStatus?.Split(new[] { " | " }, StringSplitOptions.None).Select(x => x.Trim()).ToArray();
            var averageIncomes = accountProfile.AverageIncome?.Split(new[] { " | " }, StringSplitOptions.None).Select(x => x.Trim()).ToArray();
            var targetAverageIncomes = targetAccountProfile.AverageIncome?.Split(new[] { " | " }, StringSplitOptions.None).Select(x => x.Trim()).ToArray();
            var educationLevels = accountProfile.EducationLevel?.Split(new[] { " | " }, StringSplitOptions.None).Select(x => x.Trim()).ToArray();
            var targetEducationLevels = targetAccountProfile.EducationLevel?.Split(new[] { " | " }, StringSplitOptions.None).Select(x => x.Trim()).ToArray();
            var jobFields = accountProfile.JobField?.Split(new[] { " | " }, StringSplitOptions.None).Select(x => x.Trim()).ToArray();
            var targetJobFields = targetAccountProfile.JobField?.Split(new[] { " | " }, StringSplitOptions.None).Select(x => x.Trim()).ToArray();


            // in tất cả ra
            Console.WriteLine($"CountryRegions: {string.Join(", ", countryRegions ?? new string[] { "null" })}");
            Console.WriteLine($"TargetCountryRegions: {string.Join(", ", targetCountryRegions ?? new string[] { "null" })}");
            Console.WriteLine("kêt quả so sánh: " +
                (countryRegions == null || targetCountryRegions == null || countryRegions.Any(cr => targetCountryRegions.Contains(cr))));
            Console.WriteLine($"MaritalStatuses: {string.Join(", ", maritalStatuses ?? new string[] { "null" })}");
            Console.WriteLine($"TargetMaritalStatuses: {string.Join(", ", targetMaritalStatuses ?? new string[] { "null" })}");
            Console.WriteLine("kêt quả so sánh: " +
                (maritalStatuses == null || targetMaritalStatuses == null || maritalStatuses.Any(ms => targetMaritalStatuses.Contains(ms))));
            Console.WriteLine($"AverageIncomes: {string.Join(", ", averageIncomes ?? new string[] { "null" })}");
            Console.WriteLine($"TargetAverageIncomes: {string.Join(", ", targetAverageIncomes ?? new string[] { "null" })}");
            Console.WriteLine("kêt quả so sánh: " +
                (averageIncomes == null || targetAverageIncomes == null || averageIncomes.Any(ai => targetAverageIncomes.Contains(ai))));
            Console.WriteLine($"EducationLevels: {string.Join(", ", educationLevels ?? new string[] { "null" })}");
            Console.WriteLine($"TargetEducationLevels: {string.Join(", ", targetEducationLevels ?? new string[] { "null" })}");
            Console.WriteLine("kêt quả so sánh: " +
                (educationLevels == null || targetEducationLevels == null || educationLevels.Any(el => targetEducationLevels.Contains(el))));
            Console.WriteLine($"JobFields: {string.Join(", ", jobFields ?? new string[] { "null" })}");
            Console.WriteLine($"TargetJobFields: {string.Join(", ", targetJobFields ?? new string[] { "null" })}");
            Console.WriteLine("kêt quả so sánh: " +
                (jobFields == null || targetJobFields == null || jobFields.Any(jf => targetJobFields.Contains(jf))));


            return (countryRegions == null || targetCountryRegions == null || countryRegions.Any(cr => targetCountryRegions.Contains(cr)))
                && (maritalStatuses == null || targetMaritalStatuses == null || maritalStatuses.Any(ms => targetMaritalStatuses.Contains(ms)))
                && (averageIncomes == null || targetAverageIncomes == null || averageIncomes.Any(ai => targetAverageIncomes.Contains(ai)))
                && (educationLevels == null || targetEducationLevels == null || educationLevels.Any(el => targetEducationLevels.Contains(el)))
                && (jobFields == null || targetJobFields == null || jobFields.Any(jf => targetJobFields.Contains(jf)));

        }

        public async Task<bool> DeactivateAsync(int id, bool deactivate)
        {
            var account = await _appDbContext.Accounts.FindAsync(id);
            if (account == null)
            {
                throw new Exception("Account không tồn tại");
            }
            if (deactivate)
            {
                var tz = TimeZoneInfo.FindSystemTimeZoneById(_appConfig.TIME_ZONE);
                account.DeactivatedAt = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz);
            }
            else
            {
                account.DeactivatedAt = null;
            }

            _appDbContext.Entry(account).State = EntityState.Modified;

            var rowsAffected = await _appDbContext.SaveChangesAsync();
            return rowsAffected > 1;
        }
        
        public async Task<int> CountAccountRegistrationByPeriodAsync(DateOnly startDate, DateOnly endDate)
        {
            return await _appDbContext.Accounts
                .Where(account => DateOnly.FromDateTime(account.CreatedAt.Date) >= startDate &&
                                  DateOnly.FromDateTime(account.CreatedAt.Date) <= endDate)
                .CountAsync();
        }
    }
}
