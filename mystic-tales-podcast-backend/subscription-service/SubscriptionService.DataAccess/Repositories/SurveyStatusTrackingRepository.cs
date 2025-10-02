using SubscriptionService.DataAccess.Data;
using SubscriptionService.DataAccess.Repositories.interfaces;
using Microsoft.EntityFrameworkCore;
using SubscriptionService.DataAccess.Entities;

namespace SubscriptionService.DataAccess.Repositories
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
