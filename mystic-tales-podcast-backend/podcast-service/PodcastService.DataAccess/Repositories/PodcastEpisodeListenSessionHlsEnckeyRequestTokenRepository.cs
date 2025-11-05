using PodcastService.DataAccess.Data;
using PodcastService.DataAccess.Repositories.interfaces;
using Microsoft.EntityFrameworkCore;
using PodcastService.DataAccess.Entities;
using PodcastService.Common.AppConfigurations.App.interfaces;
using PodcastService.DataAccess.Entities.SqlServer;
using System.Linq.Expressions;

namespace PodcastService.DataAccess.Repositories
{
    public class PodcastEpisodeListenSessionHlsEnckeyRequestTokenRepository : IPodcastEpisodeListenSessionHlsEnckeyRequestTokenRepository
    {
        private readonly AppDbContext _appDbContext;
        private readonly IAppConfig _appConfig;

        public PodcastEpisodeListenSessionHlsEnckeyRequestTokenRepository(AppDbContext appDbContext, IAppConfig appConfig)
        {
            _appConfig = appConfig;

            _appDbContext = appDbContext;
        }

        public async Task<bool> IsUsedUpdateByTokenAndSessionIdAsync(string token, Guid sessionId, bool isUsed)
        {
            try
            {
                var entity = await _appDbContext.PodcastEpisodeListenSessionHlsEnckeyRequestTokens
                .Where(pelsert => pelsert.Token == token && pelsert.PodcastEpisodeListenSessionId == sessionId)
                .FirstOrDefaultAsync();

                if (entity == null)
                {
                    return false;
                }

                entity.IsUsed = isUsed;

                _appDbContext.PodcastEpisodeListenSessionHlsEnckeyRequestTokens.Update(entity);

                var rowsAffected = await _appDbContext.SaveChangesAsync();
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                throw new HttpRequestException("Remove content from PodcastEpisodeListenSession by EpisodeId failed, error: " + ex.Message);
            }
        }

    }
}
