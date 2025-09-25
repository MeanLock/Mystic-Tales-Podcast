using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SurveyTalkService.Common.AppConfigurations.App.interfaces;
using SurveyTalkService.Common.AppConfigurations.FilePath.interfaces;
using SurveyTalkService.DataAccess.Data;
using SurveyTalkService.DataAccess.UOW;
using SurveyTalkService.DataAccess.Repositories.interfaces;
using SurveyTalkService.DataAccess.Entities;
using Net.payOS;
using Net.payOS.Types;
using SurveyTalkService.BusinessLogic.DTOs.Transaction;
using System.Linq.Expressions;
using SurveyTalkService.Infrastructure.Configurations.Payos.interfaces;
using SurveyTalkService.BusinessLogic.Helpers.AuthHelpers;
using SurveyTalkService.BusinessLogic.Helpers.FileHelpers;

namespace SurveyTalkService.BusinessLogic.Services.DbServices.PaymentServices
{
    public class AccountPaymentService
    {
        // LOGGER
        private readonly ILogger<AccountPaymentService> _logger;

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



        public AccountPaymentService(
            ILogger<AccountPaymentService> logger,
            AppDbContext appDbContext,
            BcryptHelper bcryptHelper,
            JwtHelper jwtHelper,
            IUnitOfWork unitOfWork,

            IGenericRepository<Account> accountGenericRepository,
            IGenericRepository<AccountBalanceTransaction> accountBalanceTransactionGenericRepository,

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

        public async Task ConfirmPayment(WebhookType webhookBody)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    if (webhookBody == null)
                    {
                        throw new Exception("Invalid webhook data.");
                    }
                    if (webhookBody.data.description == "VQRIO123")
                    {
                        return;
                    }
                    // Console.WriteLine("\n\n\n" + JsonConvert.SerializeObject(webhookBody, Formatting.Indented) + "\n\n\n");

                    // Retrieve the payment history record
                    AccountBalanceTransaction? accountBalanceTransaction = await _unitOfWork.AccountBalanceTransactionRepository.FindByIdAsync((int)webhookBody.data.orderCode);
                    if (accountBalanceTransaction == null)
                    {
                        throw new HttpRequestException("Payment history not found.");
                    }

                    if (webhookBody.code == "00")
                    {
                        accountBalanceTransaction.TransactionStatusId = 2;
                    }
                    else
                    {
                        accountBalanceTransaction.TransactionStatusId = 4;
                    }

                    await _accountBalanceTransactionGenericRepository.UpdateAsync(accountBalanceTransaction.Id, accountBalanceTransaction);

                    if (accountBalanceTransaction.TransactionTypeId == 5)
                    {
                        accountBalanceTransaction.Account.Balance += accountBalanceTransaction.Amount;
                    }
                    else if (accountBalanceTransaction.TransactionTypeId == 6)
                    {
                        accountBalanceTransaction.Account.Balance -= accountBalanceTransaction.Amount;
                    }
                    await _accountGenericRepository.UpdateAsync(accountBalanceTransaction.Account.Id, accountBalanceTransaction.Account);

                    // Commit the transaction
                    await transaction.CommitAsync();
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    Console.WriteLine(ex.Message);
                    Console.WriteLine("\n" + ex.StackTrace + "\n");
                    throw new HttpRequestException("Payment confirmation failed.");
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
