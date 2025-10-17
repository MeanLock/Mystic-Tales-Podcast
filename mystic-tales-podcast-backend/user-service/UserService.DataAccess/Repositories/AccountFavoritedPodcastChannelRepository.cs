using UserService.DataAccess.Data;
using UserService.DataAccess.Repositories.interfaces;
using Microsoft.EntityFrameworkCore;
using UserService.DataAccess.Entities;
using UserService.Common.AppConfigurations.App.interfaces;
using UserService.DataAccess.Entities.SqlServer;
using System.Linq.Expressions;

namespace UserService.DataAccess.Repositories
{
    public class AccountFavoritedPodcastChannelRepository : IAccountFavoritedPodcastChannelRepository
    {
        private readonly AppDbContext _appDbContext;
        private readonly IAppConfig _appConfig;

        public AccountFavoritedPodcastChannelRepository(AppDbContext appDbContext, IAppConfig appConfig)
        {
            _appConfig = appConfig;

            _appDbContext = appDbContext;
        }


        public async Task DeleteByAccountIdAndPodcastChannelIdAsync(int accountId, Guid podcastChannelId)
        {
            var entity = await _appDbContext.AccountFavoritedPodcastChannels
                .FirstOrDefaultAsync(afp => afp.AccountId == accountId && afp.PodcastChannelId == podcastChannelId);

            if (entity != null)
            {
                _appDbContext.AccountFavoritedPodcastChannels.Remove(entity);
                await _appDbContext.SaveChangesAsync();
            }
        }
    }
}
