using UserService.DataAccess.Data;
using UserService.DataAccess.Repositories.interfaces;
using Microsoft.EntityFrameworkCore;
using UserService.DataAccess.Entities;
using UserService.Common.AppConfigurations.App.interfaces;
using UserService.DataAccess.Entities.SqlServer;
using System.Linq.Expressions;

namespace UserService.DataAccess.Repositories
{
    public class AccountSavedPodcastEpisodeRepository : IAccountSavedPodcastEpisodeRepository
    {
        private readonly AppDbContext _appDbContext;
        private readonly IAppConfig _appConfig;

        public AccountSavedPodcastEpisodeRepository(AppDbContext appDbContext, IAppConfig appConfig)
        {
            _appConfig = appConfig;

            _appDbContext = appDbContext;
        }


        public async Task DeleteByAccountIdAndPodcastEpisodeIdAsync(int accountId, Guid podcastEpisodeId)
        {
            var entity = await _appDbContext.AccountSavedPodcastEpisodes
                .FirstOrDefaultAsync(afp => afp.AccountId == accountId && afp.PodcastEpisodeId == podcastEpisodeId);

            if (entity != null)
            {
                _appDbContext.AccountSavedPodcastEpisodes.Remove(entity);
                await _appDbContext.SaveChangesAsync();
            }
        }

        public async Task<List<int>> DeleteByPodcastEpisodeIdAsync(Guid podcastEpisodeId)
        {
            var entities = await _appDbContext.AccountSavedPodcastEpisodes
                .Where(afp => afp.PodcastEpisodeId == podcastEpisodeId)
                .ToListAsync();

            if (entities.Any())
            {
                _appDbContext.AccountSavedPodcastEpisodes.RemoveRange(entities);
                await _appDbContext.SaveChangesAsync();
            }

            return entities.Select(afp => afp.AccountId).ToList();
        }
    }
}
