using PodcastService.DataAccess.Data;
using PodcastService.DataAccess.Repositories.interfaces;
using Microsoft.EntityFrameworkCore;
using PodcastService.DataAccess.Entities;
using PodcastService.Common.AppConfigurations.App.interfaces;
using PodcastService.DataAccess.Entities.SqlServer;
using System.Linq.Expressions;

namespace PodcastService.DataAccess.Repositories
{
    public class PodcastEpisodePublishDuplicateDetectionRepository : IPodcastEpisodePublishDuplicateDetectionRepository
    {
        private readonly AppDbContext _appDbContext;
        private readonly IAppConfig _appConfig;

        public PodcastEpisodePublishDuplicateDetectionRepository(AppDbContext appDbContext, IAppConfig appConfig)
        {
            _appConfig = appConfig;

            _appDbContext = appDbContext;
        }


        public async Task<bool> DeleteByPublishReviewSessionIdAsync(int sessionId)
        {
            try
            {
                var entities = _appDbContext.PodcastEpisodePublishDuplicateDetections
                .Where(ped => ped.PodcastEpisodePublishReviewSessionId == sessionId);

                _appDbContext.PodcastEpisodePublishDuplicateDetections.RemoveRange(entities);

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
