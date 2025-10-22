using UserService.DataAccess.Repositories;
using UserService.DataAccess.Repositories.interfaces;

namespace UserService.DataAccess.UOW;
public interface IUnitOfWork
{
    IAccountRepository AccountRepository { get; }
    IAccountOnlineTrackingRepository AccountOnlineTrackingRepository { get; }
    ISurveyRepository SurveyRepository { get; }
    ISurveyQuestionRepository SurveyQuestionRepository { get; }
    ISurveyTopicFavoriteRepository SurveyTopicFavoriteRepository { get; }
    IPasswordResetTokenRepository PasswordResetTokenRepository { get; }
    ISurveyTakenResultRepository SurveyTakenResultRepository { get; }
    ISurveyStatusTrackingRepository SurveyStatusTrackingRepository { get; }
    IFilterTagRepository FilterTagRepository { get; }
    ITakerTagFilterRepository TakerTagFilterRepository { get; }
    ISystemConfigProfileRepository SystemConfigProfileRepository { get; }
    ISurveyTakerSegmentRepository SurveyTakerSegmentRepository { get; }
    ISurveyTagFilterRepository SurveyTagFilterRepository { get; }
    IAccountProfileRepository AccountProfileRepository { get; }
    IAccountBalanceTransactionRepository AccountBalanceTransactionRepository { get; }
    ISurveyCommunityTransactionRepository SurveyCommunityTransactionRepository { get; }

    int Complete();
}
