using System.Linq.Expressions;

namespace PodcastService.DataAccess.Repositories.interfaces
{
    public interface IPodcastChannelHashtagRepository
    {
        Task<bool> DeleteByPodcastChannelIdAsync(Guid channelId);
    }
}
