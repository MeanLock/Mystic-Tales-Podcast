using UserService.DataAccess.Data;
using UserService.DataAccess.Repositories.interfaces;
using Microsoft.EntityFrameworkCore;
using UserService.DataAccess.Entities;
using UserService.Common.AppConfigurations.App.interfaces;
using UserService.DataAccess.Entities.SqlServer;
using System.Linq.Expressions;

namespace UserService.DataAccess.Repositories
{
    public class AccountFollowedPodcastShowRepository : IAccountFollowedPodcastShowRepository
    {
        private readonly AppDbContext _appDbContext;
        private readonly IAppConfig _appConfig;

        public AccountFollowedPodcastShowRepository(AppDbContext appDbContext, IAppConfig appConfig)
        {
            _appConfig = appConfig;

            _appDbContext = appDbContext;
        }


        public async Task DeleteByAccountIdAndPodcastShowIdAsync(int accountId, Guid podcastShowId)
        {
            var entity = await _appDbContext.AccountFollowedPodcastShows
                .FirstOrDefaultAsync(afp => afp.AccountId == accountId && afp.PodcastShowId == podcastShowId);

            if (entity != null)
            {
                _appDbContext.AccountFollowedPodcastShows.Remove(entity);
                await _appDbContext.SaveChangesAsync();
            }
        }

        public async Task<List<int>> DeleteByPodcastShowIdAsync(Guid podcastShowId)
        {
            var entities = await _appDbContext.AccountFollowedPodcastShows
                .Where(afp => afp.PodcastShowId == podcastShowId)
                .ToListAsync();

            if (entities.Any())
            {
                _appDbContext.AccountFollowedPodcastShows.RemoveRange(entities);
                await _appDbContext.SaveChangesAsync();
            }

            return entities.Select(afp => afp.AccountId).ToList();
        }
    }
}
