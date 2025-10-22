using UserService.DataAccess.Data;
using UserService.DataAccess.Repositories.interfaces;
using Microsoft.EntityFrameworkCore;
using UserService.DataAccess.Entities;

namespace UserService.DataAccess.Repositories
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
