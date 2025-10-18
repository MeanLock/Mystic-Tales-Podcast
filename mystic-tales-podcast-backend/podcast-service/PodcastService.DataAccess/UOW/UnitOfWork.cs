using Google;
using PodcastService.DataAccess.Data;
using PodcastService.DataAccess.Repositories.interfaces;

namespace PodcastService.DataAccess.UOW;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _appDbContext;
    // public IAccountRepository AccountRepository { get; }
    public IPodcastChannelHashtagRepository PodcastChannelHashtagRepository { get; }
    public IPodcastShowHashtagRepository PodcastShowHashtagRepository { get; }

    public UnitOfWork(
        AppDbContext appDbContext,
        // IAccountRepository accountRepository,
        IPodcastChannelHashtagRepository podcastChannelHashtagRepository,
        IPodcastShowHashtagRepository podcastShowHashtagRepository

        )
    {
        _appDbContext = appDbContext;
        PodcastChannelHashtagRepository = podcastChannelHashtagRepository;
        PodcastShowHashtagRepository = podcastShowHashtagRepository;

        // this.AccountRepository = accountRepository;
        // this.AccountOnlineTrackingRepository = accountOnlineTrackingRepository;
        // this.SurveyRepository = surveyRepository;
        // this.SurveyQuestionRepository = surveyQuestionRepository;
        // this.SurveyTopicFavoriteRepository = surveyTopicFavoriteRepository;
        // this.PasswordResetTokenRepository = passwordResetTokenRepository;
        // this.SurveyTakenResultRepository = surveyTakenResultRepository;
        // this.SurveyStatusTrackingRepository = surveyStatusTrackingRepository;
        // this.FilterTagRepository = filterTagRepository;
        // this.TakerTagFilterRepository = takerTagFilterRepository;
        // this.SystemConfigProfileRepository = systemConfigProfileRepository;
        // this.SurveyTakerSegmentRepository = surveyTakerSegmentRepository;
        // this.SurveyTagFilterRepository = surveyTagFilterRepository;
        // this.AccountProfileRepository = accountProfileRepository;
        // this.AccountBalanceTransactionRepository = accountBalanceTransactionRepository;
        // this.SurveyCommunityTransactionRepository = surveyCommunityTransactionRepository;
    }

    public int Complete()
    {
        return _appDbContext.SaveChanges();
    }
}
