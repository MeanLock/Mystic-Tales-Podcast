using PodcastService.DataAccess.Data;
using PodcastService.DataAccess.Repositories.interfaces;
using Microsoft.EntityFrameworkCore;
using PodcastService.DataAccess.Entities;
using PodcastService.Common.AppConfigurations.App.interfaces;
using PodcastService.DataAccess.Entities.SqlServer;
using System.Linq.Expressions;

namespace PodcastService.DataAccess.Repositories
{
    public class PodcastShowHashtagRepository : IPodcastShowHashtagRepository
    {
        private readonly AppDbContext _appDbContext;
        private readonly IAppConfig _appConfig;

        public PodcastShowHashtagRepository(AppDbContext appDbContext, IAppConfig appConfig)
        {
            _appConfig = appConfig;

            _appDbContext = appDbContext;
        }


        public async Task<bool> DeleteByPodcastShowIdAsync(Guid showId)
        {
            try
            {
                var entities = _appDbContext.PodcastShowHashtags
                .Where(psh => psh.PodcastShowId == showId);

                _appDbContext.PodcastShowHashtags.RemoveRange(entities);

                var rowsAffected = await _appDbContext.SaveChangesAsync();
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                throw new HttpRequestException("Delete PodcastShowHashtag by ShowId failed, error: " + ex.Message);
            }

        }


    }
}
