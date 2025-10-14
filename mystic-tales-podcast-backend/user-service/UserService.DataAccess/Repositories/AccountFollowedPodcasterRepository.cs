using UserService.DataAccess.Data;
using UserService.DataAccess.Repositories.interfaces;
using Microsoft.EntityFrameworkCore;
using UserService.DataAccess.Entities;
using UserService.Common.AppConfigurations.App.interfaces;
using UserService.DataAccess.Entities.SqlServer;
using System.Linq.Expressions;

namespace UserService.DataAccess.Repositories
{
    public class AccountFollowedPodcasterRepository : IAccountFollowedPodcasterRepository
    {
        private readonly AppDbContext _appDbContext;
        private readonly IAppConfig _appConfig;

        public AccountFollowedPodcasterRepository(AppDbContext appDbContext, IAppConfig appConfig)
        {
            _appConfig = appConfig;

            _appDbContext = appDbContext;
        }


        public async Task DeleteByAccountIdAndPodcasterIdAsync(int accountId, int podcasterId)
        {
            var entity = await _appDbContext.AccountFollowedPodcasters
                .FirstOrDefaultAsync(afp => afp.AccountId == accountId && afp.PodcasterId == podcasterId);

            if (entity != null)
            {
                _appDbContext.AccountFollowedPodcasters.Remove(entity);
                await _appDbContext.SaveChangesAsync();
            }
        }
    }
}
