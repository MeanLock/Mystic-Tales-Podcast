using PodcastService.DataAccess.Repositories;
using PodcastService.DataAccess.Repositories.interfaces;

namespace PodcastService.DataAccess.UOW;
public interface IUnitOfWork
{
    // IAccountRepository AccountRepository { get; }
    IPodcastChannelHashtagRepository PodcastChannelHashtagRepository { get; }
    IPodcastShowHashtagRepository PodcastShowHashtagRepository { get; }
    IPodcastEpisodeHashtagRepository PodcastEpisodeHashtagRepository { get; }
    IPodcastEpisodeIllegalContentTypeMarkingRepository PodcastEpisodeIllegalContentTypeMarkingRepository { get; }
    IPodcastEpisodePublishDuplicateDetectionRepository PodcastEpisodePublishDuplicateDetectionRepository { get; }
    IPodcastEpisodeListenSessionRepository PodcastEpisodeListenSessionRepository { get; }
    IPodcastShowReviewRepository PodcastShowReviewRepository { get; }
    IPodcastEpisodeListenSessionHlsEnckeyRequestTokenRepository PodcastEpisodeListenSessionHlsEnckeyRequestTokenRepository { get; }

    int Complete();
}
