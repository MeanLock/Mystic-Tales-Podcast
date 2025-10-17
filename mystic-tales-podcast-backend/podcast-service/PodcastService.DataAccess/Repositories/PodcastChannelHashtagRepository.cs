using PodcastService.DataAccess.Data;
using PodcastService.DataAccess.Repositories.interfaces;
using Microsoft.EntityFrameworkCore;
using PodcastService.DataAccess.Entities;
using PodcastService.Common.AppConfigurations.App.interfaces;
using PodcastService.DataAccess.Entities.SqlServer;
using System.Linq.Expressions;

namespace PodcastService.DataAccess.Repositories
{
    public class PodcastChannelHashtagRepository : IPodcastChannelHashtagRepository
    {
        private readonly AppDbContext _appDbContext;
        private readonly IAppConfig _appConfig;

        public PodcastChannelHashtagRepository(AppDbContext appDbContext, IAppConfig appConfig)
        {
            _appConfig = appConfig;

            _appDbContext = appDbContext;
        }


        public async Task<bool> DeleteByPodcastChannelIdAsync(Guid channelId)
        {
            try
            {
                var entities = _appDbContext.PodcastChannelHashtags
                .Where(pch => pch.PodcastChannelId == channelId);

                _appDbContext.PodcastChannelHashtags.RemoveRange(entities);

                var rowsAffected = await _appDbContext.SaveChangesAsync();
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                throw new HttpRequestException("Delete PodcastChannelHashtag by ChannelId failed, error: " + ex.Message);
            }

        }


    }
}
