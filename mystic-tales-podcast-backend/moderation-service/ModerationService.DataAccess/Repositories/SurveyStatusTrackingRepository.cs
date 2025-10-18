using ModerationService.DataAccess.Data;
using ModerationService.DataAccess.Repositories.interfaces;
using Microsoft.EntityFrameworkCore;
using ModerationService.DataAccess.Entities;

namespace ModerationService.DataAccess.Repositories
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
