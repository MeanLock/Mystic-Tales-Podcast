using ModerationService.DataAccess.Data;
using ModerationService.DataAccess.Entities;

namespace ModerationService.DataAccess.Repositories.interfaces
{
    public interface IPasswordResetTokenRepository
    {
        Task<PasswordResetToken?> CreateAsync(PasswordResetToken passwordResetToken);
        Task<PasswordResetToken> FindByAccountIdAndToken(int accountId, string token);
        Task<PasswordResetToken> FindLatestTokenByAccountId(int accountId);
        Task UpdateAllTokensUsageByAccountId(int accountId, bool isUsed);

        Task<PasswordResetToken> getValidToken(int accountId, string token);
    }
}
