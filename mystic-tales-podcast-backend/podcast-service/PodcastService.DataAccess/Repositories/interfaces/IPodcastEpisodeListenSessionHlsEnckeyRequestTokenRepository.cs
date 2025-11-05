using System.Linq.Expressions;

namespace PodcastService.DataAccess.Repositories.interfaces
{
    public interface IPodcastEpisodeListenSessionHlsEnckeyRequestTokenRepository
    {
        Task<bool> IsUsedUpdateByTokenAndSessionIdAsync(string token, Guid sessionId, bool isUsed);
    }
}
