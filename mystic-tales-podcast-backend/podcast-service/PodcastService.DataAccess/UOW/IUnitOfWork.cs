using PodcastService.DataAccess.Repositories;
using PodcastService.DataAccess.Repositories.interfaces;

namespace PodcastService.DataAccess.UOW;
public interface IUnitOfWork
{
    // IAccountRepository AccountRepository { get; }
    IPodcastChannelHashtagRepository PodcastChannelHashtagRepository { get; }
    IPodcastShowHashtagRepository PodcastShowHashtagRepository { get; }

    int Complete();
}
