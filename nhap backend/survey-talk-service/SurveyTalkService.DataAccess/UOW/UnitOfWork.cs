using Google;
using SurveyTalkService.DataAccess.Data;
using SurveyTalkService.DataAccess.Repositories.interfaces;

namespace SurveyTalkService.DataAccess.UOW;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _appDbContext;
    public IAccountRepository AccountRepository { get; }
    public IAccountOnlineTrackingRepository AccountOnlineTrackingRepository { get; }
    public ISurveyRepository SurveyRepository { get; }
    public ISurveyQuestionRepository SurveyQuestionRepository { get; }
    public ISurveyTopicFavoriteRepository SurveyTopicFavoriteRepository { get; }
    public IPasswordResetTokenRepository PasswordResetTokenRepository { get; }
    public ISurveyTakenResultRepository SurveyTakenResultRepository { get; }
    public ISurveyStatusTrackingRepository SurveyStatusTrackingRepository { get; }
    public IFilterTagRepository FilterTagRepository { get; }
    public ITakerTagFilterRepository TakerTagFilterRepository { get; }
    public ISystemConfigProfileRepository SystemConfigProfileRepository { get; }
    public ISurveyTakerSegmentRepository SurveyTakerSegmentRepository { get; }
    public ISurveyTagFilterRepository SurveyTagFilterRepository { get; }
    public IAccountProfileRepository AccountProfileRepository { get; }
    public IAccountBalanceTransactionRepository AccountBalanceTransactionRepository { get; }
    public ISurveyCommunityTransactionRepository SurveyCommunityTransactionRepository { get; }

    public UnitOfWork(
        AppDbContext appDbContext,
        IAccountRepository accountRepository,
        IAccountOnlineTrackingRepository accountOnlineTrackingRepository,
        ISurveyRepository surveyRepository,
        ISurveyQuestionRepository surveyQuestionRepository,
        ISurveyTopicFavoriteRepository surveyTopicFavoriteRepository,
        IPasswordResetTokenRepository passwordResetTokenRepository,
        ISurveyTakenResultRepository surveyTakenResultRepository,
        ISurveyStatusTrackingRepository surveyStatusTrackingRepository,
        IFilterTagRepository filterTagRepository,
        ITakerTagFilterRepository takerTagFilterRepository,
        ISystemConfigProfileRepository systemConfigProfileRepository,
        ISurveyTakerSegmentRepository surveyTakerSegmentRepository,
        ISurveyTagFilterRepository surveyTagFilterRepository,
        IAccountProfileRepository accountProfileRepository,
        IAccountBalanceTransactionRepository accountBalanceTransactionRepository,
        ISurveyCommunityTransactionRepository surveyCommunityTransactionRepository

        )
    {
        _appDbContext = appDbContext;

        this.AccountRepository = accountRepository;
        this.AccountOnlineTrackingRepository = accountOnlineTrackingRepository;
        this.SurveyRepository = surveyRepository;
        this.SurveyQuestionRepository = surveyQuestionRepository;
        this.SurveyTopicFavoriteRepository = surveyTopicFavoriteRepository;
        this.PasswordResetTokenRepository = passwordResetTokenRepository;
        this.SurveyTakenResultRepository = surveyTakenResultRepository;
        this.SurveyStatusTrackingRepository = surveyStatusTrackingRepository;
        this.FilterTagRepository = filterTagRepository;
        this.TakerTagFilterRepository = takerTagFilterRepository;
        this.SystemConfigProfileRepository = systemConfigProfileRepository;
        this.SurveyTakerSegmentRepository = surveyTakerSegmentRepository;
        this.SurveyTagFilterRepository = surveyTagFilterRepository;
        this.AccountProfileRepository = accountProfileRepository;
        this.AccountBalanceTransactionRepository = accountBalanceTransactionRepository;
        this.SurveyCommunityTransactionRepository = surveyCommunityTransactionRepository;
    }

    public int Complete()
    {
        return _appDbContext.SaveChanges();
    }
}
