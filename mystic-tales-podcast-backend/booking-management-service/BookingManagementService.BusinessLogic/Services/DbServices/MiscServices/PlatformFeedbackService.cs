using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using BookingManagementService.Common.AppConfigurations.App.interfaces;
using BookingManagementService.Common.AppConfigurations.FilePath.interfaces;
using BookingManagementService.DataAccess.Data;
using BookingManagementService.DataAccess.UOW;
using BookingManagementService.DataAccess.Repositories.interfaces;
using BookingManagementService.DataAccess.Entities;
using Net.payOS;
using Net.payOS.Types;
using BookingManagementService.BusinessLogic.DTOs.Transaction;
using System.Linq.Expressions;
using BookingManagementService.BusinessLogic.DTOs.Auth;
using BookingManagementService.BusinessLogic.DTOs.Feedback;
using BookingManagementService.Infrastructure.Configurations.Payos.interfaces;
using BookingManagementService.BusinessLogic.Helpers.AuthHelpers;
using BookingManagementService.BusinessLogic.Helpers.FileHelpers;

namespace BookingManagementService.BusinessLogic.Services.DbServices.MiscServices
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
        IGenericRepository<Account> _accountGenericRepository;
        IGenericRepository<AccountBalanceTransaction> _accountBalanceTransactionGenericRepository;
        IGenericRepository<PlatformFeedback> _platformFeedbackGenericRepository;



        public PlatformFeedbackService(
            ILogger<PlatformFeedbackService> logger,
            AppDbContext appDbContext,
            BcryptHelper bcryptHelper,
            JwtHelper jwtHelper,
            IUnitOfWork unitOfWork,

            IGenericRepository<Account> accountGenericRepository,
            IGenericRepository<AccountBalanceTransaction> accountBalanceTransactionGenericRepository,
            IGenericRepository<PlatformFeedback> platformFeedbackGenericRepository,

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

            _accountGenericRepository = accountGenericRepository;
            _accountBalanceTransactionGenericRepository = accountBalanceTransactionGenericRepository;
            _platformFeedbackGenericRepository = platformFeedbackGenericRepository;

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

        public async Task CreatePlatformFeedback(PlatformFeedbackRequestDTO platformFeedback, Account account)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    // Check if the account has already given feedback
                    var existingFeedback = await _platformFeedbackGenericRepository.FindAll(
                        predicate: feedback => feedback.AccountId == account.Id
                    ).FirstOrDefaultAsync();
                    if (existingFeedback != null)
                    {
                        // update
                        existingFeedback.RatingScore = platformFeedback.Feedback.RatingScore;
                        existingFeedback.Comment = platformFeedback.Feedback.Comment;

                        await _platformFeedbackGenericRepository.UpdateAsync(existingFeedback.AccountId, existingFeedback);
                    }
                    else
                    {
                        PlatformFeedback platformFeedbackEntity = new PlatformFeedback
                        {
                            AccountId = account.Id,
                            RatingScore = platformFeedback.Feedback.RatingScore,
                            Comment = platformFeedback.Feedback.Comment
                        };
                        // Save feedback to the database
                        await _platformFeedbackGenericRepository.CreateAsync(platformFeedbackEntity);
                    }



                    // Commit the transaction
                    await transaction.CommitAsync();
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    Console.WriteLine(ex.Message);
                    Console.WriteLine("\n" + ex.StackTrace + "\n");
                    throw new HttpRequestException("Tạo phản hồi không thành công, lỗi: " + ex.Message);
                }
            }
        }
    
    }
}
