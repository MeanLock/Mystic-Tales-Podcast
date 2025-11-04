using UserService.DataAccess.Data;
using UserService.DataAccess.Repositories.interfaces;
using Microsoft.EntityFrameworkCore;
using UserService.DataAccess.Entities;
using UserService.Common.AppConfigurations.App.interfaces;
using UserService.DataAccess.Entities.SqlServer;
using System.Linq.Expressions;

namespace UserService.DataAccess.Repositories
{
    public class PodcastBuddyReviewRepository : IPodcastBuddyReviewRepository
    {
        private readonly AppDbContext _appDbContext;
        private readonly IAppConfig _appConfig;

        public PodcastBuddyReviewRepository(AppDbContext appDbContext, IAppConfig appConfig)
        {
            _appConfig = appConfig;

            _appDbContext = appDbContext;
        }

        public async Task<List<int>> DeleteByPodcasterIdAsync(int podcasterId)
        {
            var entities = await _appDbContext.PodcastBuddyReviews
                .Where(afp => afp.PodcastBuddyId == podcasterId)
                .ToListAsync();

            if (entities.Any())
            {
                _appDbContext.PodcastBuddyReviews.RemoveRange(entities);
                await _appDbContext.SaveChangesAsync();
            }

            return entities.Select(afp => afp.AccountId).ToList();
        }
    }
}
