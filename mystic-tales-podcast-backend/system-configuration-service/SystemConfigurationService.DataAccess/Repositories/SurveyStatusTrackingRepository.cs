using SystemConfigurationService.DataAccess.Data;
using SystemConfigurationService.DataAccess.Repositories.interfaces;
using Microsoft.EntityFrameworkCore;
using SystemConfigurationService.DataAccess.Entities;

namespace SystemConfigurationService.DataAccess.Repositories
{
    public class SurveyStatusTrackingRepository : ISurveyStatusTrackingRepository
    {
        private readonly AppDbContext _appDbContext;

        public SurveyStatusTrackingRepository(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        
    }
}
