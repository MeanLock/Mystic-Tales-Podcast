using PodcastService.DataAccess.Data;
using PodcastService.DataAccess.Repositories.interfaces;
using Microsoft.EntityFrameworkCore;
using PodcastService.DataAccess.Entities;
using PodcastService.Common.AppConfigurations.App.interfaces;
using PodcastService.DataAccess.Entities.SqlServer;
using System.Linq.Expressions;

namespace PodcastService.DataAccess.Repositories
{
    public class PodcastEpisodeListenSessionRepository : IPodcastEpisodeListenSessionRepository
    {
        private readonly AppDbContext _appDbContext;
        private readonly IAppConfig _appConfig;

        public PodcastEpisodeListenSessionRepository(AppDbContext appDbContext, IAppConfig appConfig)
        {
            _appConfig = appConfig;

            _appDbContext = appDbContext;
        }

        public async Task<bool> DeleteByPodcastEpisodeIdAsync(Guid episodeId)
        {
            try
            {
                var entities = _appDbContext.PodcastEpisodeListenSessions
                .Where(pels => pels.PodcastEpisodeId == episodeId);

                _appDbContext.PodcastEpisodeListenSessions.RemoveRange(entities);

                var rowsAffected = await _appDbContext.SaveChangesAsync();
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                throw new HttpRequestException("Delete PodcastEpisodeListenSession by EpisodeId failed, error: " + ex.Message);
            }

        }


    }
}
