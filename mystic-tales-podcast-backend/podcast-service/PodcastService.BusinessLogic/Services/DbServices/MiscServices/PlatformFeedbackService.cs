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
using Net.payOS;
using Net.payOS.Types;
using PodcastService.BusinessLogic.DTOs.Transaction;
using System.Linq.Expressions;
using PodcastService.BusinessLogic.DTOs.Auth;
using PodcastService.BusinessLogic.DTOs.Feedback;
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
        public async Task<string> CreateAccountBalanceDepositPaymentLink(AccountBalanceDepositDTO accountBalanceDepositDTO, int accountId)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    PayOS payOS = new PayOS(_payosConfig.ClientID, _payosConfig.APIKey, _payosConfig.ChecksumKey);





                    AccountBalanceTransaction accountBalanceTransaction = new AccountBalanceTransaction
                    {
                        AccountId = accountId,
                        Amount = accountBalanceDepositDTO.Amount,
                        TransactionTypeId = 5,
                        TransactionStatusId = 1,
                    };

                    // Save payment history to the database
                    AccountBalanceTransaction newAccountBalanceTransaction = await _accountBalanceTransactionGenericRepository.CreateAsync(accountBalanceTransaction);
                    long orderCode = newAccountBalanceTransaction.Id;

                    PaymentData paymentData = new PaymentData(orderCode, (int)accountBalanceDepositDTO.Amount, $"SURVEYTALK NAP TIEN",
                                             [], accountBalanceDepositDTO.CancelUrl, accountBalanceDepositDTO.ReturnUrl);

                    CreatePaymentResult createPayment = await payOS.createPaymentLink(paymentData);


                    // Commit the transaction
                    await transaction.CommitAsync();
                    return createPayment.checkoutUrl;

                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    Console.WriteLine(ex.Message);
                    Console.WriteLine("\n" + ex.StackTrace + "\n");
                    throw new HttpRequestException("Tạo liên kết thanh toán không thành công.");
                }
            }

        }

        public async Task WithdrawAccountBalance(Account account, AccountBalanceWithdrawalDTO accountBalanceWithdrawalDTO)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    if (account.Balance < accountBalanceWithdrawalDTO.Amount)
                    {
                        throw new Exception("Số dư không đủ để rút tiền.");
                    }

                    // AccountBalanceTransaction accountBalanceTransaction = new AccountBalanceTransaction
                    // {
                    //     AccountId = account.Id,
                    //     Amount = accountBalanceWithdrawalDTO.Amount,
                    //     TransactionTypeId = 6, // Rút tiền
                    //     TransactionStatusId = 1, // Chờ xử lý
                    //     BankAccountNumber = accountBalanceWithdrawalDTO.BankAccountNumber,
                    //     BankCode = accountBalanceWithdrawalDTO.BankCode,
                    //     Description = accountBalanceWithdrawalDTO.Description
                    // };

                    // Save withdrawal request to the database
                    // await _accountBalanceTransactionGenericRepository.CreateAsync(accountBalanceTransaction);

                    // Update account balance
                    account.Balance -= accountBalanceWithdrawalDTO.Amount;
                    await _accountGenericRepository.UpdateAsync(account.Id, account);

                    // Commit the transaction
                    await transaction.CommitAsync();
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    Console.WriteLine(ex.Message);
                    Console.WriteLine("\n" + ex.StackTrace + "\n");
                    throw new HttpRequestException("Rút tiền không thành công, lỗi: " + ex.Message);
                }
            }
        }

        public async Task<List<AccountBalanceTransactionDTO>> GetAccountBalanceDepositHistory(Account account)
        {
            try
            {

                // Retrieve the deposit history for the account
                var transactions = await _accountBalanceTransactionGenericRepository.FindAll(
                    predicate: transaction => transaction.TransactionTypeId == 5 && (transaction.TransactionStatusId != 3 && transaction.TransactionStatusId != 4)
                    &&
                    // nếu là manager thì lấy hết account, nếu là customer thì chỉ lấy account của mình
                    (account.RoleId == 2 || account.RoleId == 4 && transaction.AccountId == account.Id)
                    ,
                    includeProperties: new Expression<Func<AccountBalanceTransaction, object>>[] {
                        t => t.Account,
                        t => t.TransactionType,
                        t => t.TransactionStatus
                    }
                ).ToListAsync();

                var history = new List<AccountBalanceTransactionDTO>();
                foreach (var t in transactions)
                {
                    history.Add(new AccountBalanceTransactionDTO
                    {
                        Id = t.Id,
                        Account = new AccountBalanceTransactionDTOAccountDTO
                        {
                            Id = t.Account.Id,
                            Email = t.Account.Email,
                            FullName = t.Account.FullName,
                            Phone = t.Account.Phone,
                            MainImageUrl = await _fileIOHelper.GeneratePresignedUrlAsync(FilePathHelper.CombinePaths(_filePathConfig.ACCOUNt_IMAGE_PATH, t.Account.Id.ToString(), "main"))
                        },
                        Amount = t.Amount,
                        CreatedAt = t.CreatedAt,
                        TransactionStatus = new TransactionStatusDTO
                        {
                            Id = t.TransactionStatus.Id,
                            Name = t.TransactionStatus.Name
                        },
                        TransactionType = new TransactionTypeDTO
                        {
                            Id = t.TransactionType.Id,
                            Name = t.TransactionType.Name,
                            OperationType = t.TransactionType.OperationType
                        }
                    });
                }

                return history;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine("\n" + ex.StackTrace + "\n");
                throw new HttpRequestException("Lấy lịch sử nạp tiền không thành công, lỗi: " + ex.Message);
            }

        }

        public async Task<List<AccountBalanceTransactionDTO>> GetAccountBalanceWithdrawalHistory(Account account)
        {
            try
            {
                // Retrieve the withdraw history for the account
                var transactions = await _accountBalanceTransactionGenericRepository.FindAll(
                    predicate: transaction => transaction.TransactionTypeId == 6 && (transaction.TransactionStatusId != 3 && transaction.TransactionStatusId != 4)
                    &&
                    // nếu là manager thì lấy hết account, nếu là customer thì chỉ lấy account của mình
                    (account.RoleId == 2 || account.RoleId == 4 && transaction.AccountId == account.Id)
                    ,
                    includeProperties: new Expression<Func<AccountBalanceTransaction, object>>[] {
                        t => t.Account,
                        t => t.TransactionType,
                        t => t.TransactionStatus
                    }
                ).ToListAsync();
                var history = new List<AccountBalanceTransactionDTO>();
                foreach (var t in transactions)
                {
                    history.Add(new AccountBalanceTransactionDTO
                    {
                        Id = t.Id,
                        Account = new AccountBalanceTransactionDTOAccountDTO
                        {
                            Id = t.Account.Id,
                            Email = t.Account.Email,
                            FullName = t.Account.FullName,
                            Phone = t.Account.Phone,
                            MainImageUrl = await _fileIOHelper.GeneratePresignedUrlAsync(FilePathHelper.CombinePaths(_filePathConfig.ACCOUNt_IMAGE_PATH, t.Account.Id.ToString(), "main"))
                        },
                        Amount = t.Amount,
                        CreatedAt = t.CreatedAt,
                        TransactionStatus = new TransactionStatusDTO
                        {
                            Id = t.TransactionStatus.Id,
                            Name = t.TransactionStatus.Name
                        },
                        TransactionType = new TransactionTypeDTO
                        {
                            Id = t.TransactionType.Id,
                            Name = t.TransactionType.Name,
                            OperationType = t.TransactionType.OperationType
                        }
                    });
                }
                return history;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine("\n" + ex.StackTrace + "\n");
                throw new HttpRequestException("Lấy lịch sử rút tiền không thành công, lỗi: " + ex.Message);
            }
        }
    }
}
