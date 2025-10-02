using PodcastService.DataAccess.Data;
using PodcastService.DataAccess.Entities;

namespace PodcastService.DataAccess.Repositories.interfaces
{
    public interface ITakerTagFilterRepository
    {
        Task UpdateAsync(TakerTagFilter takerTagFilter);
        
    }
}
