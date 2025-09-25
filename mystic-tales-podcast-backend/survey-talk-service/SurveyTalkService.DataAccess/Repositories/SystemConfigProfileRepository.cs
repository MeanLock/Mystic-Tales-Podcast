using SurveyTalkService.DataAccess.Data;
using SurveyTalkService.DataAccess.Repositories.interfaces;
using Microsoft.EntityFrameworkCore;
using SurveyTalkService.DataAccess.Entities;

namespace SurveyTalkService.DataAccess.Repositories
{
    public class SystemConfigProfileRepository : ISystemConfigProfileRepository
    {
        private readonly AppDbContext _appDbContext;

        public SystemConfigProfileRepository(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        public async Task<SystemConfigProfile> FindActiveProfileAsync()
        {
            var activeProfile = await _appDbContext.SystemConfigProfiles
                .Include(p => p.AccountGeneralConfig)
                .Include(p => p.AccountLevelSettingConfigs)
                .Include(p => p.SurveyGeneralConfig)
                .Include(p => p.SurveyTimeRateConfigs)
                .Include(p => p.SurveyMarketConfig)
                .Include(p => p.SurveySecurityModeConfigs)
                .Where(p => p.IsActive)
                .FirstOrDefaultAsync();

            if (activeProfile == null)
            {
                throw new InvalidOperationException("Không tìm thấy cấu hình hệ thống đang hoạt động.");
            }

            return activeProfile;
        }
    }
}
