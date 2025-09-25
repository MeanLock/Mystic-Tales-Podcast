using SurveyTalkService.DataAccess.Data;
using SurveyTalkService.DataAccess.Repositories.interfaces;
using Microsoft.EntityFrameworkCore;
using SurveyTalkService.DataAccess.Entities;
using SurveyTalkService.Common.AppConfigurations.App.interfaces;

namespace SurveyTalkService.DataAccess.Repositories
{
    public class PasswordResetTokenRepository : IPasswordResetTokenRepository
    {
        private readonly AppDbContext _appDbContext;
        private readonly IAppConfig _appConfig;

        public PasswordResetTokenRepository(AppDbContext appDbContext, IAppConfig appConfig)
        {
            _appConfig = appConfig;
            _appDbContext = appDbContext;
        }

        public async Task<PasswordResetToken?> CreateAsync(PasswordResetToken passwordResetToken)
        {
            // Check IsUsed = true ở tất cả các PasswordResetToken của accountId
            await UpdateAllTokensUsageByAccountId(passwordResetToken.AccountId, true);


            _appDbContext.PasswordResetTokens.Add(passwordResetToken);
            await _appDbContext.SaveChangesAsync();

            return passwordResetToken;
        }



        public async Task<PasswordResetToken> FindByAccountIdAndToken(int accountId, string token)
        {
            // Tìm kiếm PasswordResetToken theo AccountId và Token
            return await _appDbContext.PasswordResetTokens
                .FirstOrDefaultAsync(prt => prt.AccountId == accountId && prt.Token == token);
        }

        public async Task UpdateAllTokensUsageByAccountId(int accountId, bool isUsed)
        {
            // Cập nhật tất cả PasswordResetToken của accountId với IsUsed = true
            var tokens = await _appDbContext.PasswordResetTokens
                .Where(prt => prt.AccountId == accountId)
                .ToListAsync();

            foreach (var token in tokens)
            {
                token.IsUsed = isUsed;
            }

            _appDbContext.PasswordResetTokens.UpdateRange(tokens);
            await _appDbContext.SaveChangesAsync();
        }

        public async Task<PasswordResetToken> FindLatestTokenByAccountId(int accountId)
        {
            // Tìm kiếm PasswordResetToken mới nhất theo AccountId
            return await _appDbContext.PasswordResetTokens
                .Where(prt => prt.AccountId == accountId)
                .OrderByDescending(prt => prt.CreatedAt)
                .FirstOrDefaultAsync();
        }

        public async Task<PasswordResetToken> getValidToken(int accountId, string token)
        {
            // Kiểm tra xem token có hợp lệ hay không
            var latestToken = await FindLatestTokenByAccountId(accountId);
            var passwordResetToken = await FindByAccountIdAndToken(accountId, token);
            var tz = TimeZoneInfo.FindSystemTimeZoneById(_appConfig.TIME_ZONE);
            var now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz);
            if (passwordResetToken == null || passwordResetToken.IsUsed == true || passwordResetToken.ExpiredAt < now )
            {
                throw new Exception("token không hợp lệ hoặc đã hết hạn");
            }
            
            if(latestToken == null)
            {
                throw new Exception("không tìm thấy token mới nhất cho tài khoản này");
            }else if (latestToken.Id != passwordResetToken.Id)
            {
                throw new Exception("token không hợp lệ");
            }

            return passwordResetToken;
        }
    }
}
