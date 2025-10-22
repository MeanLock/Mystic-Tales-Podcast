using PodcastService.DataAccess.Data;
using PodcastService.DataAccess.Repositories.interfaces;
using Microsoft.EntityFrameworkCore;
using PodcastService.DataAccess.Entities;

namespace PodcastService.DataAccess.Repositories
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
