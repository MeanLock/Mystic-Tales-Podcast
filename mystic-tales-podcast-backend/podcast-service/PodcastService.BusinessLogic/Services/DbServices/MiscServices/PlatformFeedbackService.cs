using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PodcastService.Common.AppConfigurations.App.interfaces;
using PodcastService.Common.AppConfigurations.FilePath.interfaces;
using PodcastService.DataAccess.Data;
using PodcastService.DataAccess.UOW;
using PodcastService.DataAccess.Repositories.interfaces;
using PodcastService.DataAccess.Entities;
using PodcastService.Infrastructure.Configurations.Payos.interfaces;
using PodcastService.BusinessLogic.Helpers.AuthHelpers;
using PodcastService.BusinessLogic.Helpers.FileHelpers;

namespace PodcastService.BusinessLogic.Services.DbServices.MiscServices
{
    public class PlatformFeedbackService
    {
        // LOGGER
        private readonly ILogger<PlatformFeedbackService> _logger;

        // CONFIG
        public readonly IAppConfig _appConfig;
        private readonly IFilePathConfig _filePathConfig;
        private readonly IPayosConfig _payosConfig;

        // DB CONTEXT
        private readonly AppDbContext _appDbContext;

        // HELPERS
        private readonly BcryptHelper _bcryptHelper;
        private readonly JwtHelper _jwtHelper;
        private readonly FileIOHelper _fileIOHelper;

        // UNIT OF WORK
        private readonly IUnitOfWork _unitOfWork;

        // REPOSITORIES
        // IGenericRepository<Account> _accountGenericRepository;
        // IGenericRepository<AccountBalanceTransaction> _accountBalanceTransactionGenericRepository;
        // IGenericRepository<PlatformFeedback> _platformFeedbackGenericRepository;



        public PlatformFeedbackService(
            ILogger<PlatformFeedbackService> logger,
            AppDbContext appDbContext,
            BcryptHelper bcryptHelper,
            JwtHelper jwtHelper,
            IUnitOfWork unitOfWork,

            // IGenericRepository<Account> accountGenericRepository,
            // IGenericRepository<AccountBalanceTransaction> accountBalanceTransactionGenericRepository,
            // IGenericRepository<PlatformFeedback> platformFeedbackGenericRepository,

            FileIOHelper fileIOHelper,
            IFilePathConfig filePathConfig,
            IAppConfig appConfig,
            IPayosConfig payosConfig
            )
        {
            _logger = logger;
            _appDbContext = appDbContext;
            _bcryptHelper = bcryptHelper;
            _jwtHelper = jwtHelper;
            _unitOfWork = unitOfWork;

            // _accountGenericRepository = accountGenericRepository;
            // _accountBalanceTransactionGenericRepository = accountBalanceTransactionGenericRepository;
            // _platformFeedbackGenericRepository = platformFeedbackGenericRepository;

            _fileIOHelper = fileIOHelper;
            _filePathConfig = filePathConfig;
            _appConfig = appConfig;
            _payosConfig = payosConfig;
        }
        public static long GenerateRandomLong(int minDigits = 5, int maxDigits = 15)
        {
            var random = new Random();
            int length = random.Next(minDigits, maxDigits + 1);
            long min = (long)Math.Pow(10, length - 1);
            long max = (long)Math.Pow(10, length) - 1;
            return random.NextInt64(min, max);
        }

        /////////////////////////////////////////////////////////////


    }
}
