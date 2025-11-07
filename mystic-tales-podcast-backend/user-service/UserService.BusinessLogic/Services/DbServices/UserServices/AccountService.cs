using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using UserService.Common.AppConfigurations.App.interfaces;
using UserService.Common.AppConfigurations.FilePath.interfaces;
using UserService.DataAccess.Data;
using UserService.DataAccess.UOW;
using UserService.DataAccess.Repositories.interfaces;
using UserService.BusinessLogic.DTOs.Auth;
using UserService.BusinessLogic.DTOs.Account;
using UserService.Common.AppConfigurations.BusinessSetting.interfaces;
using UserService.BusinessLogic.Models.Mail;
using UserService.Infrastructure.Services.Google.Email;
using UserService.Infrastructure.Configurations.Google.interfaces;
using UserService.BusinessLogic.DTOs.MessageQueue.UserManagementDomain.CreateAccount;
using UserService.BusinessLogic.Helpers.AuthHelpers;
using UserService.BusinessLogic.Helpers.FileHelpers;
using UserService.BusinessLogic.Helpers.DateHelpers;
using UserService.BusinessLogic.Services.CrossServiceServices.QueryServices;
using UserService.BusinessLogic.Models.CrossService;
using Newtonsoft.Json.Linq;
using UserService.Infrastructure.Services.Kafka;
using UserService.BusinessLogic.Enums.Kafka;
using UserService.Infrastructure.Models.Kafka;
using UserService.BusinessLogic.Services.MessagingServices.interfaces;
using Confluent.Kafka;
using UserService.BusinessLogic.DTOs.MessageQueue.UserManagementDomain.SendUserServiceEmail;
using UserService.BusinessLogic.DTOs.MessageQueue.UserManagementDomain.ChangeAccountStatus;
using UserService.Infrastructure.Services.Redis;
using UserService.BusinessLogic.DTOs.Cache;
using UserService.BusinessLogic.DTOs.MessageQueue.UserManagementDomain.CreatePodcasterProfile;
using UserService.DataAccess.Entities.SqlServer;
using UserService.BusinessLogic.DTOs.MessageQueue.UserManagementDomain.UpdatePodcasterProfile;
using UserService.BusinessLogic.DTOs.MessageQueue.UserManagementDomain.UpdateUser;
using UserService.BusinessLogic.DTOs.MessageQueue.UserManagementDomain.DeactivateAccount;
using UserService.BusinessLogic.DTOs.MessageQueue.UserManagementDomain.ActivateAccount;
using UserService.BusinessLogic.DTOs.MessageQueue.UserManagementDomain.AddAccountViolationPoint;
using UserService.BusinessLogic.DTOs.MessageQueue.UserManagementDomain.VerifyPodcaster;
using UserService.BusinessLogic.DTOs.MessageQueue.UserManagementDomain.CreatePodcasterFollowed;
using UserService.BusinessLogic.DTOs.MessageQueue.UserManagementDomain.DeletePodcasterFollowed;
using UserService.BusinessLogic.DTOs.MessageQueue.UserManagementDomain.CreateChannelFavorited;
using UserService.BusinessLogic.DTOs.Channel;
using UserService.BusinessLogic.DTOs.MessageQueue.UserManagementDomain.CreateChannelFavoritedRollback;
using UserService.BusinessLogic.DTOs.MessageQueue.UserManagementDomain.DeleteChannelFavorited;
using UserService.BusinessLogic.DTOs.MessageQueue.UserManagementDomain.DeleteChannelFavoritedRollback;
using Microsoft.EntityFrameworkCore.Update;
using UserService.BusinessLogic.Services.DbServices.MiscServices;
using UserService.BusinessLogic.DTOs.MessageQueue.UserManagementDomain.CreateShowFollowed;
using UserService.BusinessLogic.Enums.Podcast;
using UserService.BusinessLogic.DTOs.Show;
using UserService.BusinessLogic.DTOs.MessageQueue.UserManagementDomain.CreateShowFollowedRollback;
using UserService.BusinessLogic.DTOs.MessageQueue.UserManagementDomain.DeleteShowFollowed;
using UserService.BusinessLogic.DTOs.MessageQueue.PublicReviewManagementDomain.CreatePodcastBuddyReview;
using UserService.BusinessLogic.DTOs.MessageQueue.PublicReviewManagementDomain.UpdatePodcastBuddyReview;
using UserService.BusinessLogic.DTOs.MessageQueue.PublicReviewManagementDomain.DeletePodcastBuddyReview;
using UserService.BusinessLogic.DTOs.MessageQueue.UserManagementDomain.CreateEpisodeSaved;
using UserService.BusinessLogic.DTOs.Episode;
using UserService.BusinessLogic.DTOs.MessageQueue.UserManagementDomain.CreateEpisodeSavedRollback;
using UserService.BusinessLogic.DTOs.MessageQueue.UserManagementDomain.DeleteEpisodeSaved;
using UserService.BusinessLogic.DTOs.MessageQueue.UserManagementDomain.DeleteEpisodeSavedRollback;
using UserService.BusinessLogic.DTOs.MessageQueue.UserManagementDomain.AddAccountBalanceAmount;
using UserService.BusinessLogic.DTOs.MessageQueue.UserManagementDomain.SubtractAccountBalanceAmount;
using UserService.BusinessLogic.DTOs.MessageQueue.UserManagementDomain.AddPodcasterBalanceAmount;
using UserService.BusinessLogic.DTOs.MessageQueue.UserManagementDomain.AddAccountBalanceAmountRollback;
using UserService.BusinessLogic.DTOs.MessageQueue.UserManagementDomain.SubtractAccountBalanceAmountRollback;
using UserService.BusinessLogic.DTOs.MessageQueue.UserManagementDomain.AddPodcasterBalanceAmountRollback;
using UserService.BusinessLogic.DTOs.MessageQueue.UserManagementDomain.SubtractAccountListenSlot;
using UserService.BusinessLogic.DTOs.MessageQueue.UserManagementDomain.AddPodcasterListenCount;
using UserService.BusinessLogic.DTOs.MessageQueue.UserManagementDomain.DeleteAccountSavedEpisodeDMCARemoveEpisodeForce;
using UserService.BusinessLogic.DTOs.MessageQueue.UserManagementDomain.DeleteAccountSavedEpisodeUnpublishEpisodeForce;
using UserService.BusinessLogic.DTOs.MessageQueue.UserManagementDomain.DeleteAccountFollowedShowDMCARemoveShowForce;
using UserService.BusinessLogic.DTOs.MessageQueue.UserManagementDomain.DeleteAccountFollowedShowUnpublishShowForce;
using UserService.BusinessLogic.DTOs.MessageQueue.UserManagementDomain.DeleteAccountSavedShowEpisodesDMCARemoveShowForce;
using UserService.BusinessLogic.DTOs.MessageQueue.UserManagementDomain.DeleteAccountSavedShowEpisodesUnpublishShowForce;
using UserService.BusinessLogic.DTOs.MessageQueue.UserManagementDomain.DeleteAccountFavoritedChannelUnpublishChannelForce;
using UserService.BusinessLogic.DTOs.MessageQueue.UserManagementDomain.DeleteAccountSavedEpisodeEpisodeDeletionForce;
using UserService.BusinessLogic.DTOs.MessageQueue.UserManagementDomain.DeleteAccountFollowedShowShowDeletionForce;
using UserService.BusinessLogic.DTOs.MessageQueue.UserManagementDomain.DeleteAccountSavedShowEpisodesShowDeletionForce;
using UserService.BusinessLogic.DTOs.MessageQueue.UserManagementDomain.DeleteAccountFavoritedChannelChannelDeletionForce;
using UserService.BusinessLogic.DTOs.MessageQueue.UserManagementDomain.DeleteAccountFollowedChannelShowsUnpublishChannelForce;
using UserService.BusinessLogic.DTOs.MessageQueue.UserManagementDomain.DeleteAccountFollowedChannelShowsChannelDeletionForce;
using UserService.BusinessLogic.DTOs.MessageQueue.UserManagementDomain.DeleteAccountSavedChannelEpisodesUnpublishChannelForce;
using UserService.BusinessLogic.DTOs.MessageQueue.UserManagementDomain.DeleteAccountSavedChannelEpisodesChannelDeletionForce;
using UserService.BusinessLogic.DTOs.MessageQueue.PublicReviewManagementDomain.DeleteBuddyReviewTerminatePodcasterForce;
using UserService.BusinessLogic.DTOs.MessageQueue.UserManagementDomain.DeleteAccountFollowedPodcasterTerminatePodcasterForce;
using UserService.BusinessLogic.DTOs.MessageQueue.UserManagementDomain.DeleteAccountFavoritedPodcasterChannelsTerminatePodcasterForce;
using UserService.BusinessLogic.DTOs.MessageQueue.UserManagementDomain.DeleteAccountFollowedPodcasterShowsTerminatePodcasterForce;
using UserService.BusinessLogic.DTOs.MessageQueue.UserManagementDomain.DeleteAccountSavedPodcasterEpisodesTerminatePodcasterForce;
using UserService.BusinessLogic.DTOs.Account.ListItems;

namespace UserService.BusinessLogic.Services.DbServices.UserServices
{
    public class AccountService
    {
        // LOGGER
        private readonly ILogger<AccountService> _logger;

        // CONFIG
        public readonly IAppConfig _appConfig;
        private readonly IFilePathConfig _filePathConfig;
        private readonly IAccountConfig _accountConfig;
        private readonly IGoogleMailConfig _googleMailConfig;

        // DB CONTEXT
        private readonly AppDbContext _appDbContext;

        // HELPERS
        private readonly BcryptHelper _bcryptHelper;
        private readonly JwtHelper _jwtHelper;
        private readonly FileIOHelper _fileIOHelper;
        private readonly DateHelper _dateHelper;

        // UNIT OF WORK
        private readonly IUnitOfWork _unitOfWork;

        // REPOSITORIES
        private readonly IGenericRepository<Account> _accountGenericRepository;
        private readonly IGenericRepository<Role> _roleGenericRepository;
        private readonly IGenericRepository<PodcasterProfile> _podcasterProfileGenericRepository;
        private readonly IGenericRepository<PodcastBuddyReview> _podcastBuddyReviewGenericRepository;
        private readonly IGenericRepository<AccountFollowedPodcaster> _accountFollowedPodcasterGenericRepository;
        private readonly IGenericRepository<AccountFavoritedPodcastChannel> _accountFavoritedPodcastChannelGenericRepository;
        private readonly IGenericRepository<AccountFollowedPodcastShow> _accountFollowedPodcastShowGenericRepository;
        private readonly IGenericRepository<AccountSavedPodcastEpisode> _accountSavedPodcastEpisodeGenericRepository;

        private readonly HttpServiceQueryClient _httpServiceQueryClient;

        // CACHING SERVICE
        private readonly AccountCachingService _accountCachingService;

        // GOOGLE SERVICE
        private readonly FluentEmailService _fluentEmailService;

        // KAFKA SERVICE
        private readonly IMessagingService _messagingService;
        private readonly KafkaProducerService _kafkaProducerService;

        // REDIS SERVICE
        private readonly RedisSharedCacheService _redisSharedCacheService;

        public AccountService(
            ILogger<AccountService> logger,
            AppDbContext appDbContext,
            BcryptHelper bcryptHelper,
            FluentEmailService fluentEmailService,
            JwtHelper jwtHelper,
            IUnitOfWork unitOfWork,

            IServiceProvider serviceProvider,
            IGenericRepository<Account> accountGenericRepository,
            IGenericRepository<Role> roleGenericRepository,
            IGenericRepository<PodcasterProfile> podcasterProfileGenericRepository,
            IGenericRepository<PodcastBuddyReview> podcastBuddyReviewGenericRepository,
            IGenericRepository<AccountFollowedPodcaster> accountFollowedPodcasterGenericRepository,
            IGenericRepository<AccountFavoritedPodcastChannel> accountFavoritedPodcastChannelGenericRepository,
            IGenericRepository<AccountFollowedPodcastShow> accountFollowedPodcastShowGenericRepository,
            IGenericRepository<AccountSavedPodcastEpisode> accountSavedPodcastEpisodeGenericRepository,

            AccountCachingService accountCachingService,

            FileIOHelper fileIOHelper,
            DateHelper dateHelper,

            IFilePathConfig filePathConfig,
            IGoogleMailConfig googleMailConfig,
            IAppConfig appConfig,
            IAccountConfig accountConfig,

            HttpServiceQueryClient httpServiceQueryClient,
            IMessagingService messagingService,
            KafkaProducerService kafkaProducerService,

            RedisSharedCacheService redisSharedCacheService
            )
        {
            _logger = logger;

            _appDbContext = appDbContext;
            _unitOfWork = unitOfWork;

            _accountGenericRepository = accountGenericRepository;
            _roleGenericRepository = roleGenericRepository;
            _podcasterProfileGenericRepository = podcasterProfileGenericRepository;
            _podcastBuddyReviewGenericRepository = podcastBuddyReviewGenericRepository;
            _accountFollowedPodcasterGenericRepository = accountFollowedPodcasterGenericRepository;
            _accountFavoritedPodcastChannelGenericRepository = accountFavoritedPodcastChannelGenericRepository;
            _accountFollowedPodcastShowGenericRepository = accountFollowedPodcastShowGenericRepository;
            _accountSavedPodcastEpisodeGenericRepository = accountSavedPodcastEpisodeGenericRepository;

            _accountCachingService = accountCachingService;

            _fileIOHelper = fileIOHelper;
            _jwtHelper = jwtHelper;
            _bcryptHelper = bcryptHelper;
            _dateHelper = dateHelper;

            _fluentEmailService = fluentEmailService;

            _filePathConfig = filePathConfig;
            _accountConfig = accountConfig;
            _googleMailConfig = googleMailConfig;
            _appConfig = appConfig;

            _httpServiceQueryClient = httpServiceQueryClient;
            _messagingService = messagingService;
            _kafkaProducerService = kafkaProducerService;

            _redisSharedCacheService = redisSharedCacheService;
        }

        public async Task<Account> GetExistAccountById(int accountId)
        {
            // var account = await _unitOfWork.AccountRepository.FindByIdAsync(accountId);
            var account = await _accountGenericRepository.FindByIdAsync(accountId);
            if (account == null)
            {
                throw new Exception("Account with id " + accountId + " does not exist");
            }
            return account;
        }

        public async Task<Account> GetExistAccountByEmail(string email)
        {
            var account = await _unitOfWork.AccountRepository.FindByEmailAsync(email);
            if (account == null)
            {
                throw new Exception("không tìm thấy tài khoản có email: " + email);
            }
            return account;
        }

        public async Task<Role> GetExistRoleById(int roleId)
        {
            var role = await _roleGenericRepository.FindByIdAsync(roleId);
            if (role == null)
            {
                throw new Exception("không tìm thấy role có id " + roleId);
            }
            return role;
        }

        public static string GenerateRandomVerifyCode(int length)
        {
            var random = new Random();
            var digits = new char[length];

            for (int i = 0; i < length; i++)
            {
                digits[i] = (char)('0' + random.Next(0, 10));
            }

            return new string(digits);
        }

        public async Task<JObject> GetActiveSystemConfigProfile()
        {
            var batchRequest = new BatchQueryRequest
            {
                Queries = new List<BatchQueryItem>
                    {
                        new BatchQueryItem
                        {
                            Key = "activeSystemConfigProfile",
                            QueryType = "findall",
                            EntityType = "SystemConfigProfile",
                                Parameters = JObject.FromObject(new
                                {
                                    where = new
                                    {
                                        IsActive = true
                                    },
                                    include = "AccountConfig,AccountViolationLevelConfigs, BookingConfig, PodcastSubscriptionConfigs, PodcastSuggestionConfig, ReviewSessionConfig",

                                }),
                            Fields = new[] { "Id", "Name", "IsActive", "AccountConfig", "AccountViolationLevelConfigs", "BookingConfig", "PodcastSubscriptionConfigs", "PodcastSuggestionConfig", "ReviewSessionConfig" }
                        }
                    }
            };
            var result = await _httpServiceQueryClient.ExecuteBatchAsync("SystemConfigurationService", batchRequest);

            return ((JArray)result.Results["activeSystemConfigProfile"]).First as JObject;
        }

        public async Task<Guid> SendChangeAccountStatusMessage(int id)
        {
            var requestData = JObject.FromObject(new
            {
                Id = id
            });
            var startSagaTriggerMessage = _kafkaProducerService.PrepareStartSagaTriggerMessage("user-management-domain", requestData, null, "account-status-change-flow");
            await _messagingService.SendSagaMessageAsync(startSagaTriggerMessage);
            return startSagaTriggerMessage.SagaInstanceId;
        }

        public int CalculateViolationLevel(int violationPoint, JArray accountViolationLevelConfigs)
        {
            int violationLevel = 0;
            accountViolationLevelConfigs = new JArray(accountViolationLevelConfigs.OrderBy(c => c.Value<int>("ViolationPointThreshold")));
            int maxLevelPointThreshold = accountViolationLevelConfigs.Max(c => c.Value<int>("ViolationPointThreshold"));
            if (violationPoint > maxLevelPointThreshold)
            {
                violationLevel = accountViolationLevelConfigs.Max(c => c.Value<int>("ViolationLevel"));
                return violationLevel;
            }
            foreach (var config in accountViolationLevelConfigs)
            {
                int level = config.Value<int>("ViolationLevel");
                int pointThreshold = config.Value<int>("ViolationPointThreshold");
                // Console.WriteLine($"Checking level {level} with threshold {pointThreshold} against violation point {violationPoint}");
                if (violationPoint <= pointThreshold)
                {
                    violationLevel = level;
                    break;
                }
            }

            return violationLevel;
        }

        public bool IsViolationLevelMax(int violationLevel, JArray accountViolationLevelConfigs)
        {
            int maxViolationLevel = accountViolationLevelConfigs.Max(c => c.Value<int>("ViolationLevel"));
            return violationLevel >= maxViolationLevel;
        }

        /////////////////////////////////////////////////////////////



        public async Task RegisterAccount(CreateAccountParameterDTO accountRegisterDTO, SagaCommandMessage command)
        {
            var registerInfo = accountRegisterDTO;
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var existAccount = await _unitOfWork.AccountRepository.FindByEmailAsync(registerInfo.Email);

                    var activeSystemConfigProfile = await GetActiveSystemConfigProfile();

                    if (existAccount != null)
                    {
                        if (existAccount.IsVerified == true)
                        {
                            throw new Exception("Account with email " + registerInfo.Email + " already exists and is verified.");
                        }
                        else
                        {
                            string verifyCode = GenerateRandomVerifyCode(_accountConfig.VerifyCodeLength);

                            existAccount.Email = registerInfo.Email;
                            existAccount.Password = _bcryptHelper.HashPassword(registerInfo.Password);
                            existAccount.FullName = registerInfo.FullName;
                            existAccount.RoleId = registerInfo.RoleId;
                            existAccount.Dob = DateOnly.FromDateTime(registerInfo.Dob);
                            existAccount.Gender = registerInfo.Gender;
                            existAccount.Address = registerInfo.Address;
                            existAccount.Phone = registerInfo.Phone;
                            existAccount.IsVerified = registerInfo.RoleId == 1 ? false : true;
                            existAccount.VerifyCode = registerInfo.RoleId == 1 ? verifyCode : null;
                            existAccount.PodcastListenSlot = registerInfo.RoleId == 1 ? activeSystemConfigProfile["AccountConfig"].Value<int?>("PodcastListenSlotThreshold") : null;
                            existAccount.MainImageFileKey = null;
                            // await _fluentEmailService.SendEmail(registerInfo.Email, new VerifyCodeEmailViewModel
                            // {
                            //     Email = registerInfo.Email,
                            //     FullName = registerInfo.FullName,
                            //     VerifyCode = verifyCode
                            // }, _googleMailConfig.AccountVerification_TemplateViewPath
                            // , _googleMailConfig.AccountVerification_MailSubject);

                            if (registerInfo.RoleId == 1)
                            {
                                var mailSendingRequestData = JObject.FromObject(new
                                {
                                    SendUserServiceEmailMailInfo = new
                                    {
                                        MailTypeName = "CustomerRegistrationVerification",
                                        ToEmail = registerInfo.Email,
                                        MailObject = new CustomerRegistrationVerificationMailViewModel
                                        {
                                            Email = registerInfo.Email,
                                            FullName = registerInfo.FullName,
                                            VerifyCode = verifyCode
                                        }
                                    }
                                });
                                var mailSendingFlow = _kafkaProducerService.PrepareStartSagaTriggerMessage(
                                    topic: KafkaTopicEnum.UserManagementDomain,
                                    requestData: mailSendingRequestData,
                                    sagaInstanceId: null,
                                    messageName: "user-service-mail-sending-flow");
                                await _messagingService.SendSagaMessageAsync(mailSendingFlow);
                            }


                            await _accountGenericRepository.UpdateAsync(existAccount.Id, existAccount);

                        }
                    }
                    else
                    {
                        string verifyCode = GenerateRandomVerifyCode(_accountConfig.VerifyCodeLength);

                        existAccount = new Account
                        {
                            Email = registerInfo.Email,
                            Password = _bcryptHelper.HashPassword(registerInfo.Password),
                            FullName = registerInfo.FullName,
                            RoleId = registerInfo.RoleId,
                            Dob = DateOnly.FromDateTime(registerInfo.Dob),
                            Gender = registerInfo.Gender,
                            Address = registerInfo.Address,
                            Phone = registerInfo.Phone,
                            IsVerified = registerInfo.RoleId == 1 ? false : true,
                            VerifyCode = registerInfo.RoleId == 1 ? verifyCode : null,
                            PodcastListenSlot = registerInfo.RoleId == 1 ? activeSystemConfigProfile["AccountConfig"].Value<int?>("PodcastListenSlotThreshold") : null,
                            MainImageFileKey = null,
                        };

                        // await _fluentEmailService.SendEmail(registerInfo.Email, new VerifyCodeEmailViewModel
                        // {
                        //     Email = registerInfo.Email,
                        //     FullName = registerInfo.FullName,
                        //     VerifyCode = verifyCode
                        // }, _googleMailConfig.AccountVerification_TemplateViewPath
                        // , _googleMailConfig.AccountVerification_MailSubject);

                        if (registerInfo.RoleId == 1)
                        {
                            // var mailSendingRequestData = JObject.FromObject(new
                            // {
                            //     MailTypeName = "CustomerRegistrationVerification",
                            //     ToEmail = registerInfo.Email,
                            //     MailObject = new CustomerRegistrationVerificationMailViewModel
                            //     {
                            //         Email = registerInfo.Email,
                            //         FullName = registerInfo.FullName,
                            //         VerifyCode = verifyCode
                            //     }
                            // });
                            var mailSendingRequestData = JObject.FromObject(new
                            {
                                SendUserServiceEmailMailInfo = new
                                {
                                    MailTypeName = "CustomerRegistrationVerification",
                                    ToEmail = registerInfo.Email,
                                    MailObject = new CustomerRegistrationVerificationMailViewModel
                                    {
                                        Email = registerInfo.Email,
                                        FullName = registerInfo.FullName,
                                        VerifyCode = verifyCode
                                    }
                                }
                            });
                            var mailSendingFlow = _kafkaProducerService.PrepareStartSagaTriggerMessage(
                                    topic: KafkaTopicEnum.UserManagementDomain,
                                    requestData: mailSendingRequestData,
                                    sagaInstanceId: null,
                                    messageName: "user-service-mail-sending-flow");
                            await _messagingService.SendSagaMessageAsync(mailSendingFlow);
                        }

                        await _accountGenericRepository.CreateAsync(existAccount);
                    }



                    var folderPath = _filePathConfig.ACCOUNT_FILE_PATH + "\\" + existAccount.Id;
                    if (registerInfo.MainImageFileKey != null && registerInfo.MainImageFileKey != "")
                    {
                        var MainImageFileKey = FilePathHelper.CombinePaths(folderPath, $"main_image{FilePathHelper.GetExtension(registerInfo.MainImageFileKey)}");
                        await _fileIOHelper.CopyFileToFileAsync(registerInfo.MainImageFileKey, MainImageFileKey);
                        await _fileIOHelper.DeleteFileAsync(registerInfo.MainImageFileKey);
                        existAccount.MainImageFileKey = MainImageFileKey;
                        await _accountGenericRepository.UpdateAsync(existAccount.Id, existAccount);
                    }

                    await transaction.CommitAsync();

                    // var messageNextRequestData = JObject.FromObject(new
                    // {
                    //     Email = existAccount.Email,
                    //     FullName = existAccount.FullName,
                    //     Dob = existAccount.Dob?.ToString("yyyy-MM-dd"),
                    //     Gender = existAccount.Gender,
                    //     Address = existAccount.Address,
                    //     Phone = existAccount.Phone,
                    //     MainImageFileKey = existAccount.MainImageFileKey,
                    //     RoleId = existAccount.RoleId,
                    //     Password = existAccount.Password,
                    // });
                    var messageNextRequestData = command.RequestData;
                    messageNextRequestData["Email"] = existAccount.Email;
                    messageNextRequestData["FullName"] = existAccount.FullName;
                    messageNextRequestData["Dob"] = existAccount.Dob?.ToString("yyyy-MM-dd");
                    messageNextRequestData["Gender"] = existAccount.Gender;
                    messageNextRequestData["Address"] = existAccount.Address;
                    messageNextRequestData["Phone"] = existAccount.Phone;
                    messageNextRequestData["MainImageFileKey"] = existAccount.MainImageFileKey;
                    messageNextRequestData["RoleId"] = existAccount.RoleId;
                    messageNextRequestData["Password"] = existAccount.Password;

                    var messageResponseData = JObject.FromObject(new
                    {
                        AccountId = existAccount.Id,
                        // Message = "Register account successfully"
                        VerifyCode = existAccount.VerifyCode,
                        Email = existAccount.Email
                    });
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.UserManagementDomain,
                        requestData: messageNextRequestData,
                        responseData: messageResponseData,
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "create-account.success"
                        );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);
                    await SendChangeAccountStatusMessage(existAccount.Id);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();

                    // xoá file tạm
                    if (registerInfo.MainImageFileKey != null && registerInfo.MainImageFileKey != "")
                    {
                        await _fileIOHelper.DeleteFileAsync(registerInfo.MainImageFileKey);
                    }

                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.UserManagementDomain,
                        requestData: command.RequestData,
                        responseData: JObject.FromObject(new
                        {
                            ErrorMessage = $"Account registration failed, error: {ex.Message}"
                        }),
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "create-account.failed"
                        );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);

                    Console.WriteLine("\n" + ex.StackTrace + "\n");
                }
            }

        }

        public async Task<List<AccountListItemResponseDTO>> GetCustomerAccounts()
        {
            try
            {
                var customers = await _unitOfWork.AccountRepository.FindByRoleIdAsync(1, null, a => a.Include(ac => ac.Role));

                var result = customers.Select(item =>
                {
                    return new AccountListItemResponseDTO
                    {
                        Id = item.Id,
                        Email = item.Email,
                        Role = new RoleDTO
                        {
                            Id = item.Role.Id,
                            Name = item.Role.Name
                        },
                        FullName = item.FullName,
                        Dob = item.Dob?.ToString("yyyy-MM-dd"),
                        Gender = item.Gender,
                        Address = item.Address,
                        Phone = item.Phone,
                        Balance = item.Balance,
                        IsVerified = item.IsVerified,
                        PodcastListenSlot = item.PodcastListenSlot,
                        ViolationPoint = item.ViolationPoint,
                        ViolationLevel = item.ViolationLevel,
                        LastPodcastListenSlotChanged = item.LastPodcastListenSlotChanged?.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                        LastViolationPointChanged = item.LastViolationPointChanged?.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                        LastViolationLevelChanged = item.LastViolationLevelChanged?.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                        MainImageFileKey = item.MainImageFileKey,
                        DeactivatedAt = item.DeactivatedAt?.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                        CreatedAt = item.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                        UpdatedAt = item.UpdatedAt.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),

                    };
                });

                return result.ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine("\n" + ex.StackTrace + "\n");
                // throw new HttpRequestException("Lấy danh sách tài khoản khách hàng thất bại, lỗi: " + ex.Message);
                throw new Exception("Get customer account list failed, error: " + ex.Message);
            }

        }

        public async Task<List<AccountListItemResponseDTO>> GetStaffAccounts(bool? IsDeactivated = null)
        {
            try
            {
                List<int> roles = new List<int> { 2 }; // 2: Head, 3: Assignee
                var staffs = await _unitOfWork.AccountRepository.FindByRoleIdsAsync(roles,
                    predicate: IsDeactivated.HasValue ? (a => (IsDeactivated == true ? a.DeactivatedAt != null : a.DeactivatedAt == null)) : null
                , a => a.Include(ac => ac.Role));

                var result = staffs.Select(item =>
                {
                    return new AccountListItemResponseDTO
                    {
                        Id = item.Id,
                        Email = item.Email,
                        Role = new RoleDTO
                        {
                            Id = item.Role.Id,
                            Name = item.Role.Name
                        },
                        FullName = item.FullName,
                        Dob = item.Dob?.ToString("yyyy-MM-dd"),
                        Gender = item.Gender,
                        Address = item.Address,
                        Phone = item.Phone,
                        Balance = item.Balance,
                        IsVerified = item.IsVerified,
                        PodcastListenSlot = item.PodcastListenSlot,
                        ViolationPoint = item.ViolationPoint,
                        ViolationLevel = item.ViolationLevel,
                        LastPodcastListenSlotChanged = item.LastPodcastListenSlotChanged?.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                        LastViolationPointChanged = item.LastViolationPointChanged?.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                        LastViolationLevelChanged = item.LastViolationLevelChanged?.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                        MainImageFileKey = item.MainImageFileKey,
                        DeactivatedAt = item.DeactivatedAt?.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                        CreatedAt = item.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                        UpdatedAt = item.UpdatedAt.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                    };
                });

                return result.ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine("\n" + ex.StackTrace + "\n");
                throw new HttpRequestException("Get staff account list failed, error: " + ex.Message);
            }

        }

        public async Task<List<PodcasterListItemResponseDTO>> GetPodcasterAccounts()
        {
            try
            {

                var podcasters = await _unitOfWork.AccountRepository.FindByRoleIdAsync(1,
                    predicate: a => a.PodcasterProfile != null,
                        a => a.Include(ac => ac.Role)
                                .Include(ac => ac.PodcasterProfile)  // Expression riêng biệt
                                .Include(ac => ac.PodcastBuddyReviewPodcastBuddies)
                                .ThenInclude(r => r.Account)
                    );


                var result = podcasters.Select(item =>
                {
                    Console.WriteLine("Số podcaster tìm thấy: " + (item.PodcastBuddyReviewPodcastBuddies.Count > 0 ? item.PodcastBuddyReviewPodcastBuddies.Count : 0));

                    return new PodcasterListItemResponseDTO
                    {
                        Id = item.Id,
                        Email = item.Email,
                        Role = new RoleDTO
                        {
                            Id = item.Role.Id,
                            Name = item.Role.Name
                        },
                        FullName = item.FullName,
                        Dob = item.Dob?.ToString("yyyy-MM-dd"),
                        Gender = item.Gender,
                        PodcastListenSlot = item.PodcastListenSlot,
                        ViolationPoint = item.ViolationPoint,
                        ViolationLevel = item.ViolationLevel,
                        LastPodcastListenSlotChanged = item.LastPodcastListenSlotChanged?.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                        LastViolationPointChanged = item.LastViolationPointChanged?.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                        LastViolationLevelChanged = item.LastViolationLevelChanged?.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                        Address = item.Address,
                        Phone = item.Phone,
                        Balance = item.Balance,
                        IsVerified = item.IsVerified,
                        MainImageFileKey = item.MainImageFileKey,
                        DeactivatedAt = item.DeactivatedAt?.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                        CreatedAt = item.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                        UpdatedAt = item.UpdatedAt.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                        PodcasterProfile = new PodcasterProfileDTO
                        {
                            AccountId = item.PodcasterProfile.AccountId,
                            Name = item.PodcasterProfile.Name,
                            Description = item.PodcasterProfile.Description,
                            AverageRating = item.PodcasterProfile.AverageRating,
                            RatingCount = item.PodcasterProfile.RatingCount,
                            TotalFollow = item.PodcasterProfile.TotalFollow,
                            ListenCount = item.PodcasterProfile.ListenCount,
                            PricePerBookingWord = item.PodcasterProfile.PricePerBookingWord,
                            CommitmentDocumentFileKey = item.PodcasterProfile.CommitmentDocumentFileKey,
                            BuddyAudioFileKey = item.PodcasterProfile.BuddyAudioFileKey,
                            OwnedBookingStorageSize = item.PodcasterProfile.OwnedBookingStorageSize,
                            UsedBookingStorageSize = item.PodcasterProfile.UsedBookingStorageSize,
                            IsVerified = item.PodcasterProfile.IsVerified,
                            CreatedAt = item.PodcasterProfile.CreatedAt,
                            UpdatedAt = item.PodcasterProfile.UpdatedAt,
                        },
                        ReviewList = item.PodcastBuddyReviewPodcastBuddies?
                        .Where(r => r.Account != null).Select(r => new ReviewListItemDTO
                        {
                            Id = r.Id,
                            Account = new AccountSnippetResponseDTO
                            {
                                Id = r.Account.Id,
                                FullName = r.Account.FullName,
                                Email = r.Account.Email,
                                MainImageFileKey = r.Account.MainImageFileKey
                            },
                            Rating = r.Rating,
                            Content = r.Content,
                            DeletedAt = r.DeletedAt,
                            PodcastBuddyId = r.PodcastBuddyId,
                            Title = r.Title,
                            UpdatedAt = r.UpdatedAt
                        }).ToList()
                    };
                });
                return result.ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine("\n" + ex.StackTrace + "\n");
                throw new HttpRequestException("Get podcaster account list failed, error: " + ex.Message);
            }
        }

        public async Task<List<PodcastBuddyListItemResponseDTO>> GetPodcastBuddyAccounts(int? requestRoleId)
        {
            try
            {

                var podcasters = await _unitOfWork.AccountRepository.FindByRoleIdAsync(1,
                    predicate: a => a.PodcasterProfile != null && a.PodcasterProfile.IsVerified == true && a.IsVerified == true,
                        a => a.Include(ac => ac.Role)
                                .Include(ac => ac.PodcasterProfile)  // Expression riêng biệt
                                .Include(ac => ac.PodcastBuddyReviewPodcastBuddies)
                                .ThenInclude(r => r.Account)
                    );

                // nếu requestRoleId là 1 thì loại bỏ các account có DeactivatedAt khác null và violation level != 0
                if (requestRoleId == null || requestRoleId == 1)
                {
                    podcasters = podcasters.Where(p => p.DeactivatedAt == null && (p.ViolationLevel == 0 || p.ViolationLevel == null)).ToList();
                }



                var result = podcasters.Select(item =>
                {
                    Console.WriteLine("Số podcaster tìm thấy: " + (item.PodcastBuddyReviewPodcastBuddies.Count > 0 ? item.PodcastBuddyReviewPodcastBuddies.Count : 0));

                    return new PodcastBuddyListItemResponseDTO
                    {
                        PodcastBuddyProfile = new PodcastBuddyProfileDTO
                        {
                            AccountId = item.PodcasterProfile.AccountId,
                            Name = item.PodcasterProfile.Name,
                            Description = item.PodcasterProfile.Description,
                            AverageRating = item.PodcasterProfile.AverageRating,
                            RatingCount = item.PodcasterProfile.RatingCount,
                            TotalFollow = item.PodcasterProfile.TotalFollow,
                            ListenCount = item.PodcasterProfile.ListenCount,
                            PricePerBookingWord = item.PodcasterProfile.PricePerBookingWord,
                            CommitmentDocumentFileKey = item.PodcasterProfile.CommitmentDocumentFileKey,
                            BuddyAudioFileKey = item.PodcasterProfile.BuddyAudioFileKey,
                            IsVerified = item.PodcasterProfile.IsVerified,
                        },
                        ReviewList = item.PodcastBuddyReviewPodcastBuddies?
                        .Where(r => r.Account != null).Select(r => new ReviewListItemDTO
                        {
                            Id = r.Id,
                            Account = new AccountSnippetResponseDTO
                            {
                                Id = r.Account.Id,
                                FullName = r.Account.FullName,
                                Email = r.Account.Email,
                                MainImageFileKey = r.Account.MainImageFileKey
                            },
                            Rating = r.Rating,
                            Content = r.Content,
                            DeletedAt = r.DeletedAt,
                            PodcastBuddyId = r.PodcastBuddyId,
                            Title = r.Title,
                            UpdatedAt = r.UpdatedAt
                        }).ToList()
                    };
                });
                return result.ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine("\n" + ex.StackTrace + "\n");
                throw new HttpRequestException("Get podcast buddy account list failed, error: " + ex.Message);
            }
        }

        public async Task<PodcasterListItemResponseDTO> GetPodcasterProfileByAccountId(int accountId)
        {
            try
            {
                var podcaster = (await _unitOfWork.AccountRepository.FindByRoleIdAsync(1,
                    predicate: a => a.PodcasterProfile != null && a.PodcasterProfile.AccountId == accountId,
                        a => a.Include(ac => ac.Role)
                                .Include(ac => ac.PodcasterProfile)  // Expression riêng biệt
                                .Include(ac => ac.PodcastBuddyReviewPodcastBuddies)
                                .ThenInclude(r => r.Account)
                    )).FirstOrDefault();

                // đếm số review
                if (podcaster == null)
                {
                    throw new Exception("Podcaster account with id " + accountId + " not found");
                }

                Console.WriteLine("Số podcaster tìm thấy: " + (podcaster.PodcastBuddyReviewPodcastBuddies.Count > 0 ? podcaster.PodcastBuddyReviewPodcastBuddies.Count : 0));

                return new PodcasterListItemResponseDTO
                {
                    Id = podcaster.Id,
                    Email = podcaster.Email,
                    Role = new RoleDTO
                    {
                        Id = podcaster.Role.Id,
                        Name = podcaster.Role.Name
                    },
                    FullName = podcaster.FullName,
                    Dob = podcaster.Dob?.ToString("yyyy-MM-dd"),
                    Gender = podcaster.Gender,
                    PodcastListenSlot = podcaster.PodcastListenSlot,
                    ViolationPoint = podcaster.ViolationPoint,
                    ViolationLevel = podcaster.ViolationLevel,
                    LastPodcastListenSlotChanged = podcaster.LastPodcastListenSlotChanged?.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                    LastViolationPointChanged = podcaster.LastViolationPointChanged?.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                    LastViolationLevelChanged = podcaster.LastViolationLevelChanged?.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                    Address = podcaster.Address,
                    Phone = podcaster.Phone,
                    Balance = podcaster.Balance,
                    IsVerified = podcaster.IsVerified,
                    MainImageFileKey = podcaster.MainImageFileKey,
                    DeactivatedAt = podcaster.DeactivatedAt?.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                    CreatedAt = podcaster.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                    UpdatedAt = podcaster.UpdatedAt.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                    PodcasterProfile = new PodcasterProfileDTO
                    {
                        AccountId = podcaster.PodcasterProfile.AccountId,
                        Name = podcaster.PodcasterProfile.Name,
                        Description = podcaster.PodcasterProfile.Description,
                        AverageRating = podcaster.PodcasterProfile.AverageRating,
                        RatingCount = podcaster.PodcasterProfile.RatingCount,
                        TotalFollow = podcaster.PodcasterProfile.TotalFollow,
                        ListenCount = podcaster.PodcasterProfile.ListenCount,
                        PricePerBookingWord = podcaster.PodcasterProfile.PricePerBookingWord,
                        CommitmentDocumentFileKey = podcaster.PodcasterProfile.CommitmentDocumentFileKey,
                        BuddyAudioFileKey = podcaster.PodcasterProfile.BuddyAudioFileKey,
                        OwnedBookingStorageSize = podcaster.PodcasterProfile.OwnedBookingStorageSize,
                        UsedBookingStorageSize = podcaster.PodcasterProfile.UsedBookingStorageSize,
                        IsVerified = podcaster.PodcasterProfile.IsVerified,
                        CreatedAt = podcaster.PodcasterProfile.CreatedAt,
                        UpdatedAt = podcaster.PodcasterProfile.UpdatedAt,
                    },
                    ReviewList = podcaster.PodcastBuddyReviewPodcastBuddies?
                    .Where(r => r.Account != null).Select(r => new ReviewListItemDTO
                    {
                        Id = r.Id,
                        Account = new AccountSnippetResponseDTO
                        {
                            Id = r.Account.Id,
                            FullName = r.Account.FullName,
                            Email = r.Account.Email,
                            MainImageFileKey = r.Account.MainImageFileKey
                        },
                        Rating = r.Rating,
                        Content = r.Content,
                        DeletedAt = r.DeletedAt,
                        PodcastBuddyId = r.PodcastBuddyId,
                        Title = r.Title,
                        UpdatedAt = r.UpdatedAt
                    }).ToList()
                };

            }
            catch (Exception ex)
            {
                Console.WriteLine("\n" + ex.StackTrace + "\n");
                throw new HttpRequestException("Get podcaster account list failed, error: " + ex.Message);
            }
        }

        public async Task<PodcastBuddyListItemResponseDTO> GetPodcastBuddyProfileByAccountId(int accountId, int requestRoleId)
        {
            try
            {
                var podcaster = (await _unitOfWork.AccountRepository.FindByRoleIdAsync(1,
                    predicate: a => a.PodcasterProfile != null && a.PodcasterProfile.AccountId == accountId && a.PodcasterProfile.IsVerified == true && a.IsVerified == true,
                        a => a.Include(ac => ac.Role)
                                .Include(ac => ac.PodcasterProfile)  // Expression riêng biệt
                                .Include(ac => ac.PodcastBuddyReviewPodcastBuddies)
                                .ThenInclude(r => r.Account)
                    )).FirstOrDefault();

                // đếm số review
                if (podcaster == null)
                {
                    throw new Exception("Podcaster account with id " + accountId + " not found");
                }

                // nếu requestRoleId là 1 thì loại bỏ các account có DeactivatedAt khác null và violation level != 0
                if (requestRoleId == 1 && (podcaster.DeactivatedAt != null || podcaster.ViolationLevel != 0))
                {
                    throw new Exception("Podcaster account with id " + accountId + " not found");
                }




                Console.WriteLine("Số podcaster tìm thấy: " + (podcaster.PodcastBuddyReviewPodcastBuddies.Count > 0 ? podcaster.PodcastBuddyReviewPodcastBuddies.Count : 0));

                return new PodcastBuddyListItemResponseDTO
                {
                    PodcastBuddyProfile = new PodcastBuddyProfileDTO
                    {
                        AccountId = podcaster.PodcasterProfile.AccountId,
                        Name = podcaster.PodcasterProfile.Name,
                        Description = podcaster.PodcasterProfile.Description,
                        AverageRating = podcaster.PodcasterProfile.AverageRating,
                        RatingCount = podcaster.PodcasterProfile.RatingCount,
                        TotalFollow = podcaster.PodcasterProfile.TotalFollow,
                        ListenCount = podcaster.PodcasterProfile.ListenCount,
                        PricePerBookingWord = podcaster.PodcasterProfile.PricePerBookingWord,
                        CommitmentDocumentFileKey = podcaster.PodcasterProfile.CommitmentDocumentFileKey,
                        BuddyAudioFileKey = podcaster.PodcasterProfile.BuddyAudioFileKey,
                        IsVerified = podcaster.PodcasterProfile.IsVerified,
                    },
                    ReviewList = podcaster.PodcastBuddyReviewPodcastBuddies?
                    .Where(r => r.Account != null).Select(r => new ReviewListItemDTO
                    {
                        Id = r.Id,
                        Account = new AccountSnippetResponseDTO
                        {
                            Id = r.Account.Id,
                            FullName = r.Account.FullName,
                            Email = r.Account.Email,
                            MainImageFileKey = r.Account.MainImageFileKey
                        },
                        Rating = r.Rating,
                        Content = r.Content,
                        DeletedAt = r.DeletedAt,
                        PodcastBuddyId = r.PodcastBuddyId,
                        Title = r.Title,
                        UpdatedAt = r.UpdatedAt
                    }).ToList()
                };

            }
            catch (Exception ex)
            {
                Console.WriteLine("\n" + ex.StackTrace + "\n");
                throw new HttpRequestException("Get podcast buddy account failed, error: " + ex.Message);
            }
        }

        public async Task ChangeAccountStatus(ChangeAccountStatusParameterDTO changeAccountStatusParameter, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var account = await _accountGenericRepository.FindByIdAsync(changeAccountStatusParameter.Id, includeProperties: a => a.PodcasterProfile);
                    if (account == null)
                    {
                        throw new Exception("Account with id " + changeAccountStatusParameter.Id + " does not exist");
                    }

                    var result = await _redisSharedCacheService.KeySetAsync<AccountStatusCache>($"account:status:{account.Id}", new AccountStatusCache
                    {
                        Id = account.Id,
                        Email = account.Email,
                        FullName = account.FullName,
                        MainImageFileKey = account.MainImageFileKey,
                        IsVerified = account.IsVerified,
                        DeactivatedAt = account.DeactivatedAt,
                        RoleId = account.RoleId,
                        LastViolationLevelChanged = account.LastViolationLevelChanged,
                        LastViolationPointChanged = account.LastViolationPointChanged,
                        ViolationLevel = account.ViolationLevel,
                        ViolationPoint = account.ViolationPoint,
                        PodcasterProfileName = account.PodcasterProfile != null ? account.PodcasterProfile.Name : null,
                        PodcasterProfileIsVerified = account.PodcasterProfile != null ? account.PodcasterProfile.IsVerified : null,
                        HasVerifiedPodcasterProfile = account.RoleId == 1 && account.PodcasterProfile != null && account.PodcasterProfile.IsVerified == true ? true : false
                    },
                    // set cache expiry to 1 hour (khi hết hạn key-value này sẽ bị xoá khỏi redis)
                    TimeSpan.FromHours(1)
                    );

                    if (result == false)
                    {
                        throw new Exception("Change account status failed, cannot update account status cache in redis");
                    }

                    var messageNextRequestData = command.RequestData;
                    messageNextRequestData["Id"] = account.Id;
                    var messageResponseData = JObject.FromObject(new
                    {
                        Id = account.Id,
                    });

                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.UserManagementDomain,
                        requestData: messageNextRequestData,
                        responseData: messageResponseData,
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "change-account-status.success"
                        );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();

                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.UserManagementDomain,
                        requestData: command.RequestData,
                        responseData: JObject.FromObject(new
                        {
                            ErrorMessage = $"Change account status failed, error: {ex.Message}"
                        }),
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "change-account-status.failed"
                        );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);

                    Console.WriteLine("\n" + ex.StackTrace + "\n");
                }
            }

        }

        public async Task CreatePodcasterProfile(CreatePodcasterProfileParameterDTO createPodcasterProfileParameter, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var activeSystemConfigProfile = await GetActiveSystemConfigProfile();
                    var account = await _accountGenericRepository.FindByIdAsync(createPodcasterProfileParameter.AccountId, includeProperties: a => a.PodcasterProfile);
                    // Console.WriteLine("Tìm thấy account: " + (account.PodcasterProfile.IsVerified == null ? "null" : account.PodcasterProfile.IsVerified.ToString()));
                    if (account.PodcasterProfile != null)
                    {
                        if (account.PodcasterProfile.IsVerified == true)
                        {
                            throw new Exception("Podcaster profile for account id " + createPodcasterProfileParameter.AccountId + " is already verified, cannot create another one");
                        }
                        else if (account.PodcasterProfile.IsVerified == null)
                        {
                            throw new Exception("Podcaster profile for account id " + createPodcasterProfileParameter.AccountId + " is pending verification, cannot create another one");
                        }
                        else
                        {
                            // xoá profile cũ
                            Console.WriteLine("Xoá podcaster profile cũ cho account id: " + account.Id);
                            await _podcasterProfileGenericRepository.DeleteAsync(account.PodcasterProfile.AccountId);
                        }
                    }



                    var podcasterProfile = new PodcasterProfile
                    {
                        AccountId = account.Id,
                        Name = createPodcasterProfileParameter.Name,
                        Description = createPodcasterProfileParameter.Description,
                        BuddyAudioFileKey = null,
                        CommitmentDocumentFileKey = null,
                        IsVerified = null,
                        OwnedBookingStorageSize = activeSystemConfigProfile["BookingConfig"].Value<double>("FreeInitialBookingStorageSize"),
                        UsedBookingStorageSize = 0,
                        RatingCount = 0
                    };


                    var folderPath = _filePathConfig.ACCOUNT_FILE_PATH + "\\" + account.Id;
                    if (createPodcasterProfileParameter.CommitmentDocumentFileKey != null && createPodcasterProfileParameter.CommitmentDocumentFileKey != "")
                    {
                        var CommitmentDocumentFileKey = FilePathHelper.CombinePaths(folderPath, $"buddy_commitment_document{FilePathHelper.GetExtension(createPodcasterProfileParameter.CommitmentDocumentFileKey)}");
                        await _fileIOHelper.CopyFileToFileAsync(createPodcasterProfileParameter.CommitmentDocumentFileKey, CommitmentDocumentFileKey);
                        await _fileIOHelper.DeleteFileAsync(createPodcasterProfileParameter.CommitmentDocumentFileKey);
                        podcasterProfile.CommitmentDocumentFileKey = CommitmentDocumentFileKey;

                    }


                    await _podcasterProfileGenericRepository.CreateAsync(podcasterProfile);
                    await transaction.CommitAsync();

                    var messageNextRequestData = JObject.FromObject(createPodcasterProfileParameter);
                    messageNextRequestData["AccountId"] = podcasterProfile.AccountId;
                    messageNextRequestData["Name"] = podcasterProfile.Name;
                    messageNextRequestData["Description"] = podcasterProfile.Description;
                    messageNextRequestData["CommitmentDocumentFileKey"] = podcasterProfile.CommitmentDocumentFileKey;

                    var messageResponseData = JObject.FromObject(new
                    {
                        // Message = "Create podcaster profile successfully",
                        AccountId = podcasterProfile.AccountId,
                        Name = podcasterProfile.Name,
                        Description = podcasterProfile.Description,
                        CommitmentDocumentFileKey = podcasterProfile.CommitmentDocumentFileKey
                    });
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.UserManagementDomain,
                        requestData: messageNextRequestData,
                        responseData: messageResponseData,
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "create-podcaster-profile.success"
                        );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);
                    await SendChangeAccountStatusMessage(podcasterProfile.AccountId);



                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();

                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.UserManagementDomain,
                        requestData: command.RequestData,
                        responseData: JObject.FromObject(new
                        {
                            ErrorMessage = $"Create podcaster profile failed, error: {ex.Message}"
                        }),
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "create-podcaster-profile.failed"
                        );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);

                    Console.WriteLine("\n" + ex.StackTrace + "\n");
                }
            }
        }

        public async Task<AccountListItemResponseDTO> GetAccountById(int accountId)
        {
            try
            {
                var account = await _accountGenericRepository.FindByIdAsync(accountId, includeProperties: a => a.Role);

                return new AccountListItemResponseDTO
                {
                    Id = account.Id,
                    Email = account.Email,
                    Role = new RoleDTO
                    {
                        Id = account.Role.Id,
                        Name = account.Role.Name
                    },
                    FullName = account.FullName,
                    Dob = account.Dob?.ToString("yyyy-MM-dd"),
                    Gender = account.Gender,
                    Address = account.Address,
                    Phone = account.Phone,
                    Balance = account.Balance,
                    IsVerified = account.IsVerified,
                    PodcastListenSlot = account.PodcastListenSlot,
                    ViolationPoint = account.ViolationPoint,
                    CreatedAt = account.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                    UpdatedAt = account.UpdatedAt.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                    MainImageFileKey = account.MainImageFileKey,
                    DeactivatedAt = account.DeactivatedAt?.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                    ViolationLevel = account.ViolationLevel,
                    LastViolationPointChanged = account.LastViolationPointChanged?.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                    LastViolationLevelChanged = account.LastViolationLevelChanged?.ToString("yyyy-MM-ddTH:mm:ss.fffZ"),
                    LastPodcastListenSlotChanged = account.LastPodcastListenSlotChanged?.ToString("yyyy-MM-ddTH:mm:ss.fffZ"),
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine("\n" + ex.StackTrace + "\n");
                throw new HttpRequestException("Get account by id failed, error: " + ex.Message);
            }


        }

        public async Task<AccountMeResponseDTO> GetAccountMe(int accountId)
        {
            try
            {
                var account = await _accountCachingService.GetAccountStatusCacheById(accountId);
                var accountFromDb = await _accountGenericRepository.FindByIdAsync(accountId,
                    includeFunc: a => a.Include(ac => ac.Role)
                    .Include(ac => ac.PodcasterProfile)  // Expression riêng biệt
                );

                if (accountFromDb == null)
                {
                    throw new Exception("Account with id " + accountId + " does not exist");
                }

                return new AccountMeResponseDTO
                {
                    Id = accountFromDb.Id,
                    Email = accountFromDb.Email,
                    FullName = accountFromDb.FullName,
                    MainImageFileKey = accountFromDb.MainImageFileKey,
                    Address = accountFromDb.Address,
                    Balance = accountFromDb.Balance,
                    DeactivatedAt = accountFromDb.DeactivatedAt?.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                    Dob = accountFromDb.Dob?.ToString("yyyy-MM-dd"),
                    Gender = accountFromDb.Gender,
                    Phone = accountFromDb.Phone,
                    PodcastListenSlot = accountFromDb.PodcastListenSlot,
                    IsPodcaster = accountFromDb.RoleId == 1 && accountFromDb.PodcasterProfile != null && accountFromDb.PodcasterProfile.IsVerified == true ? true : false
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine("\n" + ex.StackTrace + "\n");
                throw new HttpRequestException("Get account me failed, error: " + ex.Message);
            }
        }
                    
        public async Task UpdatePodcasterProfile(UpdatePodcasterProfileParameterDTO updatePodcasterProfileParameterDTO, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var podcasterProfile = (await _podcasterProfileGenericRepository.FindAll(
                        predicate: a => a.AccountId == updatePodcasterProfileParameterDTO.AccountId,
                        includeFunc: null
                        ).ToListAsync()).FirstOrDefault();


                    if (podcasterProfile == null)
                    {
                        throw new Exception("Podcaster profile with id " + updatePodcasterProfileParameterDTO.AccountId + " does not exist");
                    }
                    else if (podcasterProfile.IsVerified == false)
                    {
                        throw new Exception("Podcaster profile with id " + updatePodcasterProfileParameterDTO.AccountId + " is not verified, cannot update");
                    }




                    podcasterProfile.Name = updatePodcasterProfileParameterDTO.Name;
                    podcasterProfile.Description = updatePodcasterProfileParameterDTO.Description;

                    if (updatePodcasterProfileParameterDTO.PricePerBookingWord != null)
                    {
                        podcasterProfile.PricePerBookingWord = updatePodcasterProfileParameterDTO.PricePerBookingWord;
                    }

                    var folderPath = _filePathConfig.ACCOUNT_FILE_PATH + "\\" + podcasterProfile.AccountId;
                    if (updatePodcasterProfileParameterDTO.BuddyAudioFileKey != null && updatePodcasterProfileParameterDTO.BuddyAudioFileKey != "")
                    {
                        if (podcasterProfile.BuddyAudioFileKey != null && podcasterProfile.BuddyAudioFileKey != "")
                        {
                            await _fileIOHelper.DeleteFileAsync(podcasterProfile.BuddyAudioFileKey);
                        }
                        var BuddyAudioFileKey = FilePathHelper.CombinePaths(folderPath, $"buddy_trailer_audio{FilePathHelper.GetExtension(updatePodcasterProfileParameterDTO.BuddyAudioFileKey)}");
                        await _fileIOHelper.CopyFileToFileAsync(updatePodcasterProfileParameterDTO.BuddyAudioFileKey, BuddyAudioFileKey);
                        await _fileIOHelper.DeleteFileAsync(updatePodcasterProfileParameterDTO.BuddyAudioFileKey);
                        podcasterProfile.BuddyAudioFileKey = BuddyAudioFileKey;

                    }

                    await _podcasterProfileGenericRepository.UpdateAsync(podcasterProfile.AccountId, podcasterProfile);
                    await transaction.CommitAsync();

                    var messageNextRequestData = command.RequestData;
                    messageNextRequestData["AccountId"] = podcasterProfile.AccountId;
                    messageNextRequestData["Name"] = podcasterProfile.Name;
                    messageNextRequestData["Description"] = podcasterProfile.Description;
                    messageNextRequestData["BuddyAudioFileKey"] = podcasterProfile.BuddyAudioFileKey;
                    var messageResponseData = JObject.FromObject(new
                    {
                        // Message = "Update podcaster profile successfully",
                        AccountId = podcasterProfile.AccountId,
                        Name = podcasterProfile.Name,
                        Description = podcasterProfile.Description,
                        BuddyAudioFileKey = podcasterProfile.BuddyAudioFileKey
                    });
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.UserManagementDomain,
                        requestData: messageNextRequestData,
                        responseData: messageResponseData,
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "update-podcaster-profile.success"
                        );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);
                    await SendChangeAccountStatusMessage(podcasterProfile.AccountId);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();

                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.UserManagementDomain,
                        requestData: command.RequestData,
                        responseData: JObject.FromObject(new
                        {
                            ErrorMessage = $"Update podcaster profile failed, error: {ex.Message}"
                        }),
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "update-podcaster-profile.failed"
                        );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);

                    Console.WriteLine("\n" + ex.StackTrace + "\n");
                }
            }
        }

        public async Task UpdateUser(UpdateUserParameterDTO updateUserParameterDTO, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var updateUserInfo = updateUserParameterDTO;
                    var account = await _accountGenericRepository.FindByIdAsync(updateUserInfo.AccountId);
                    if (account == null)
                    {
                        throw new Exception("Account with id " + updateUserInfo.AccountId + " does not exist");
                    }
                    account.FullName = updateUserInfo.FullName;
                    account.Dob = DateOnly.FromDateTime(updateUserInfo.Dob);
                    account.Gender = updateUserInfo.Gender;
                    account.Address = updateUserInfo.Address;
                    account.Phone = updateUserInfo.Phone;

                    await _accountGenericRepository.UpdateAsync(account.Id, account);

                    var folderPath = _filePathConfig.ACCOUNT_FILE_PATH + "\\" + account.Id;
                    if (updateUserInfo.MainImageFileKey != null && updateUserInfo.MainImageFileKey != "")
                    {
                        if (account.MainImageFileKey != null && account.MainImageFileKey != "")
                        {
                            await _fileIOHelper.DeleteFileAsync(account.MainImageFileKey);
                        }
                        var MainImageFileKey = FilePathHelper.CombinePaths(folderPath, $"main_image{FilePathHelper.GetExtension(updateUserInfo.MainImageFileKey)}");
                        await _fileIOHelper.CopyFileToFileAsync(updateUserInfo.MainImageFileKey, MainImageFileKey);
                        await _fileIOHelper.DeleteFileAsync(updateUserInfo.MainImageFileKey);
                        account.MainImageFileKey = MainImageFileKey;
                        await _accountGenericRepository.UpdateAsync(account.Id, account);
                    }

                    await transaction.CommitAsync();

                    var messageNextRequestData = command.RequestData;
                    messageNextRequestData["AccountId"] = account.Id;
                    messageNextRequestData["Email"] = account.Email;
                    messageNextRequestData["FullName"] = account.FullName;
                    messageNextRequestData["Dob"] = account.Dob?.ToString("yyyy-MM-dd");
                    messageNextRequestData["Gender"] = account.Gender;
                    messageNextRequestData["Address"] = account.Address;
                    messageNextRequestData["Phone"] = account.Phone;
                    messageNextRequestData["MainImageFileKey"] = account.MainImageFileKey;

                    var messageResponseData = JObject.FromObject(new
                    {
                        // Message = "Update user successfully",
                        AccountId = account.Id,
                        Email = account.Email,
                        FullName = account.FullName,
                        Dob = account.Dob?.ToString("yyyy-MM-dd"),
                        Gender = account.Gender,
                        Address = account.Address,
                        Phone = account.Phone,
                        MainImageFileKey = account.MainImageFileKey
                    });
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.UserManagementDomain,
                        requestData: messageNextRequestData,
                        responseData: messageResponseData,
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "update-user.success"
                    );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);
                    await SendChangeAccountStatusMessage(account.Id);

                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();

                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.UserManagementDomain,
                        requestData: command.RequestData,
                        responseData: JObject.FromObject(new
                        {
                            ErrorMessage = $"Update user failed, error: {ex.Message}"
                        }),
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "update-user.failed"
                    );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);

                    Console.WriteLine("\n" + ex.StackTrace + "\n");
                }
            }
        }

        public async Task DeactivateAccount(DeactivateAccountParameterDTO deactivateAccountParameterDTO, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var account = await this.GetExistAccountById(deactivateAccountParameterDTO.AccountId);
                    account.DeactivatedAt = _dateHelper.GetNowByAppTimeZone();

                    await _accountGenericRepository.UpdateAsync(account.Id, account);

                    await transaction.CommitAsync();

                    var messageNextRequestData = command.RequestData;
                    messageNextRequestData["AccountId"] = account.Id;
                    var messageResponseData = JObject.FromObject(new
                    {
                        // Message = "Deactivate account successfully"
                        AccountId = account.Id,
                    });
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.UserManagementDomain,
                        requestData: messageNextRequestData,
                        responseData: messageResponseData,
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "deactivate-account.success"
                    );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);
                    await SendChangeAccountStatusMessage(account.Id);
                    // [CHỈNH SỬA SAU] CHẠY CÁC FLOW XOÁ TRONG booking, chanel/show/episode (AccountFavoritedPodcastChannel/AccountFollowedPodcastShow/AccountSavedPodcastEpisode), podcast subscription, Report review session, publish review session, DMCA Accusation, AccountFollowedPodcaster
                    JObject podcasterTerminateRequestData = JObject.FromObject(new
                    {
                        PodcasterId = account.Id
                    });
                    var startSagaTriggerMessage = _kafkaProducerService.PrepareStartSagaTriggerMessage("user-management-domain", podcasterTerminateRequestData, null, "terminate-podcaster-flow");
                    await _messagingService.SendSagaMessageAsync(startSagaTriggerMessage);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();

                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.UserManagementDomain,
                        requestData: command.RequestData,
                        responseData: JObject.FromObject(new
                        {
                            ErrorMessage = $"Deactivate account failed, error: {ex.Message}"
                        }),
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "deactivate-account.failed"
                    );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);

                    Console.WriteLine("\n" + ex.StackTrace + "\n");
                }
            }
        }

        public async Task ActivateAccount(ActivateAccountParameterDTO activateAccountParameterDTO, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var account = await this.GetExistAccountById(activateAccountParameterDTO.AccountId);
                    account.DeactivatedAt = null;

                    await _accountGenericRepository.UpdateAsync(account.Id, account);

                    await transaction.CommitAsync();


                    var messageNextRequestData = command.RequestData;
                    messageNextRequestData["AccountId"] = account.Id;
                    var messageResponseData = JObject.FromObject(new
                    {
                        // Message = "Activate account successfully"
                        AccountId = account.Id,
                    });
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.UserManagementDomain,
                        requestData: messageNextRequestData,
                        responseData: messageResponseData,
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "activate-account.success"
                    );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);
                    await SendChangeAccountStatusMessage(account.Id);

                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();

                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.UserManagementDomain,
                        requestData: command.RequestData,
                        responseData: JObject.FromObject(new
                        {
                            ErrorMessage = $"Activate account failed, error: {ex.Message}"
                        }),
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "activate-account.failed"
                    );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);

                    Console.WriteLine("\n" + ex.StackTrace + "\n");
                }
            }
        }


        public async Task AddAccountViolationPoint(AddAccountViolationPointParameterDTO addAccountViolationPointParameterDTO, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    if (addAccountViolationPointParameterDTO.ViolationPoint <= 0)
                    {
                        throw new Exception("Violation point to add must be greater than 0");
                    }
                    var activeSystemConfigProfile = await GetActiveSystemConfigProfile();
                    var account = await this.GetExistAccountById(addAccountViolationPointParameterDTO.AccountId);
                    account.ViolationPoint += addAccountViolationPointParameterDTO.ViolationPoint;
                    account.LastViolationPointChanged = _dateHelper.GetNowByAppTimeZone();

                    // cập nhật violation level nếu account violation level hiện tại là 0
                    if (account.ViolationLevel == 0)
                    {
                        var newViolationLevel = CalculateViolationLevel(account.ViolationPoint, activeSystemConfigProfile["AccountViolationLevelConfigs"] as JArray);
                        account.ViolationLevel = newViolationLevel;
                        account.LastViolationLevelChanged = _dateHelper.GetNowByAppTimeZone();
                    }
                    await _accountGenericRepository.UpdateAsync(account.Id, account);

                    await transaction.CommitAsync();


                    var messageNextRequestData = command.RequestData;
                    messageNextRequestData["AccountId"] = account.Id;
                    messageNextRequestData["ViolationPoint"] = addAccountViolationPointParameterDTO.ViolationPoint;

                    var messageResponseData = JObject.FromObject(new
                    {
                        // Message = "Add account violation point successfully"
                        AccountId = account.Id,
                        NewViolationPoint = account.ViolationPoint,
                        NewViolationLevel = account.ViolationLevel
                    });
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.UserManagementDomain,
                        requestData: messageNextRequestData,
                        responseData: messageResponseData,
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "add-account-violation-point.success"
                    );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);
                    await SendChangeAccountStatusMessage(account.Id);
                    // [CHỈNH SỬA SAU] NẾU ACCOUNT VIOLATION LEVEL == MAX VIOLATION LEVEL THÌ CHẠY CÁC FLOW XOÁ TRONG booking, chanel/show/episode (AccountFavoritedPodcastChannel/AccountFollowedPodcastShow/AccountSavedPodcastEpisode), podcast subscription, Report review session, publish review session, DMCA Accusation, AccountFollowedPodcaster
                    if (IsViolationLevelMax(account.ViolationLevel, activeSystemConfigProfile["AccountViolationLevelConfigs"] as JArray) == true)
                    {
                        JObject podcasterTerminateRequestData = JObject.FromObject(new
                        {
                            PodcasterId = account.Id
                        });
                        var startSagaTriggerMessage = _kafkaProducerService.PrepareStartSagaTriggerMessage("user-management-domain", podcasterTerminateRequestData, null, "terminate-podcaster-flow");
                        await _messagingService.SendSagaMessageAsync(startSagaTriggerMessage);
                    }
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();

                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.UserManagementDomain,
                        requestData: command.RequestData,
                        responseData: JObject.FromObject(new
                        {
                            ErrorMessage = $"Add account violation point failed, error: {ex.Message}"
                        }),
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "add-account-violation-point.failed"
                    );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);

                    Console.WriteLine("\n" + ex.StackTrace + "\n");
                }
            }
        }

        public async Task VerifyPodcaster(VerifyPodcasterParameterDTO verifyPodcasterParameterDTO, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var podcasterProfile = (await _podcasterProfileGenericRepository.FindAll(
                        predicate: a => a.AccountId == verifyPodcasterParameterDTO.AccountId,
                        includeFunc: null
                        ).ToListAsync()).FirstOrDefault();
                    if (podcasterProfile == null)
                    {
                        throw new Exception("Podcaster profile with id " + verifyPodcasterParameterDTO.AccountId + " does not exist");
                    }
                    else if (podcasterProfile.IsVerified != null)
                    {
                        throw new Exception("Podcaster profile with id " + verifyPodcasterParameterDTO.AccountId + " is already in verification process, cannot verify again");
                    }

                    podcasterProfile.IsVerified = verifyPodcasterParameterDTO.IsVerified;
                    if (podcasterProfile.IsVerified == true)
                    {
                        podcasterProfile.VerifiedAt = _dateHelper.GetNowByAppTimeZone();
                    }
                    else
                    {
                        podcasterProfile.VerifiedAt = null;
                    }
                    await _podcasterProfileGenericRepository.UpdateAsync(podcasterProfile.AccountId, podcasterProfile);
                    await transaction.CommitAsync();

                    var messageNextRequestData = command.RequestData;
                    messageNextRequestData["AccountId"] = podcasterProfile.AccountId;
                    messageNextRequestData["IsVerified"] = podcasterProfile.IsVerified;
                    var messageResponseData = JObject.FromObject(new
                    {
                        // Message = "Verify podcaster profile successfully",
                        AccountId = podcasterProfile.AccountId,
                        IsVerified = podcasterProfile.IsVerified
                    });
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.UserManagementDomain,
                        requestData: messageNextRequestData,
                        responseData: messageResponseData,
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "verify-podcaster.success"
                        );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);
                    await SendChangeAccountStatusMessage(podcasterProfile.AccountId);

                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();

                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.UserManagementDomain,
                        requestData: command.RequestData,
                        responseData: JObject.FromObject(new
                        {
                            ErrorMessage = $"Verify podcaster profile failed, error: {ex.Message}"
                        }),
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "verify-podcaster.failed"
                        );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);

                    Console.WriteLine("\n" + ex.StackTrace + "\n");
                }

            }
        }

        public async Task CreatePodcastBuddyReview(CreatePodcastBuddyReviewParameterDTO createPodcastBuddyReviewParameterDTO, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var podcasterProfile = (await _podcasterProfileGenericRepository.FindAll(
                         predicate: a => a.AccountId == createPodcastBuddyReviewParameterDTO.PodcastBuddyId && a.IsVerified == true,
                         includeFunc: null
                         ).ToListAsync()).FirstOrDefault();
                    if (podcasterProfile == null)
                    {
                        throw new Exception("Podcast buddy with id " + createPodcastBuddyReviewParameterDTO.PodcastBuddyId + " does not exist");
                    }
                    var existingReview = (await _podcastBuddyReviewGenericRepository.FindAll(
                        predicate: a => a.AccountId == createPodcastBuddyReviewParameterDTO.AccountId && a.PodcastBuddyId == createPodcastBuddyReviewParameterDTO.PodcastBuddyId,
                        includeFunc: null
                        ).ToListAsync()).FirstOrDefault();
                    if (existingReview != null)
                    {
                        throw new Exception("Account with id " + createPodcastBuddyReviewParameterDTO.AccountId + " has already reviewed podcast buddy with id " + createPodcastBuddyReviewParameterDTO.PodcastBuddyId);
                    }

                    var podcastBuddyReview = new PodcastBuddyReview
                    {
                        AccountId = createPodcastBuddyReviewParameterDTO.AccountId,
                        PodcastBuddyId = createPodcastBuddyReviewParameterDTO.PodcastBuddyId,
                        Title = createPodcastBuddyReviewParameterDTO.Title,
                        Content = createPodcastBuddyReviewParameterDTO.Content,
                        Rating = createPodcastBuddyReviewParameterDTO.Rating,
                    };

                    await _podcastBuddyReviewGenericRepository.CreateAsync(podcastBuddyReview);
                    // cập nhật rating count cho podcaster profile
                    podcasterProfile.RatingCount += 1;
                    podcasterProfile.AverageRating = ((podcasterProfile.AverageRating * (podcasterProfile.RatingCount - 1)) + createPodcastBuddyReviewParameterDTO.Rating) / podcasterProfile.RatingCount;
                    await _podcasterProfileGenericRepository.UpdateAsync(podcasterProfile.AccountId, podcasterProfile);

                    await transaction.CommitAsync();


                    var messageNextRequestData = command.RequestData;
                    messageNextRequestData["AccountId"] = podcastBuddyReview.AccountId;
                    messageNextRequestData["PodcastBuddyId"] = podcastBuddyReview.PodcastBuddyId;
                    messageNextRequestData["Title"] = podcastBuddyReview.Title;
                    messageNextRequestData["Content"] = podcastBuddyReview.Content;
                    messageNextRequestData["Rating"] = podcastBuddyReview.Rating;

                    var messageResponseData = JObject.FromObject(new
                    {
                        // Message = "Create podcast buddy review successfully",
                        PodcastBuddyReviewId = podcastBuddyReview.Id,
                        AccountId = podcastBuddyReview.AccountId,
                        PodcastBuddyId = podcastBuddyReview.PodcastBuddyId,
                        Title = podcastBuddyReview.Title,
                        Content = podcastBuddyReview.Content,
                        Rating = podcastBuddyReview.Rating
                    });
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.PublicReviewManagementDomain,
                        requestData: messageNextRequestData,
                        responseData: messageResponseData,
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "create-podcast-buddy-review.success"
                        );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);

                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();

                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.PublicReviewManagementDomain,
                        requestData: command.RequestData,
                        responseData: JObject.FromObject(new
                        {
                            ErrorMessage = $"Create podcast buddy review failed, error: {ex.Message}"
                        }),
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "create-podcast-buddy-review.failed"
                        );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);
                    Console.WriteLine("\n" + ex.StackTrace + "\n");
                }
            }
        }

        public async Task UpdatePodcastBuddyReview(UpdatePodcastBuddyReviewParameterDTO updatePodcastBuddyReviewParameterDTO, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var existingReview = await _podcastBuddyReviewGenericRepository.FindByIdAsync(updatePodcastBuddyReviewParameterDTO.PodcastBuddyReviewId);

                    if (existingReview == null)
                    {
                        throw new Exception("Account with id " + updatePodcastBuddyReviewParameterDTO.AccountId + " has not reviewed podcast buddy with id " + updatePodcastBuddyReviewParameterDTO.PodcastBuddyReviewId);
                    }
                    else if (existingReview.AccountId != updatePodcastBuddyReviewParameterDTO.AccountId)
                    {
                        throw new Exception("Account with id " + updatePodcastBuddyReviewParameterDTO.AccountId + " is not the owner of podcast buddy review with id " + updatePodcastBuddyReviewParameterDTO.PodcastBuddyReviewId);
                    }
                    var previousRating = existingReview.Rating;
                    existingReview.Title = updatePodcastBuddyReviewParameterDTO.Title ?? existingReview.Title;
                    existingReview.Content = updatePodcastBuddyReviewParameterDTO.Content ?? existingReview.Content;
                    existingReview.Rating = updatePodcastBuddyReviewParameterDTO.Rating;
                    await _podcastBuddyReviewGenericRepository.UpdateAsync(existingReview.Id, existingReview);


                    var podcasterProfile = (await _podcasterProfileGenericRepository.FindAll(
                         predicate: a => a.AccountId == existingReview.PodcastBuddyId && a.IsVerified == true,
                         includeFunc: null
                         ).ToListAsync()).FirstOrDefault();
                    if (podcasterProfile == null)
                    {
                        throw new Exception("Podcast buddy with id " + existingReview.PodcastBuddyId + " does not exist");
                    }

                    // cập nhật rating count cho podcaster profile
                    podcasterProfile.AverageRating = ((podcasterProfile.AverageRating * podcasterProfile.RatingCount) - previousRating + updatePodcastBuddyReviewParameterDTO.Rating) / podcasterProfile.RatingCount;
                    await _podcasterProfileGenericRepository.UpdateAsync(podcasterProfile.AccountId, podcasterProfile);

                    await transaction.CommitAsync();

                    var messageNextRequestData = command.RequestData;
                    messageNextRequestData["PodcastBuddyReviewId"] = existingReview.Id;
                    messageNextRequestData["AccountId"] = existingReview.AccountId;
                    messageNextRequestData["Title"] = existingReview.Title;
                    messageNextRequestData["Content"] = existingReview.Content;
                    messageNextRequestData["Rating"] = existingReview.Rating;
                    var messageResponseData = JObject.FromObject(new
                    {
                        // Message = "Create podcast buddy review successfully",
                        PodcastBuddyReviewId = existingReview.Id,
                        AccountId = existingReview.AccountId,
                        Title = existingReview.Title,
                        Content = existingReview.Content,
                        Rating = existingReview.Rating

                    });
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.PublicReviewManagementDomain,
                        requestData: messageNextRequestData,
                        responseData: messageResponseData,
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "update-podcast-buddy-review.success"
                        );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);

                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();

                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.PublicReviewManagementDomain,
                        requestData: command.RequestData,
                        responseData: JObject.FromObject(new
                        {
                            ErrorMessage = $"Update podcast buddy review failed, error: {ex.Message}"
                        }),
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "update-podcast-buddy-review.failed"
                        );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);
                    Console.WriteLine("\n" + ex.StackTrace + "\n");
                }
            }
        }

        public async Task DeletePodcastBuddyReview(DeletePodcastBuddyReviewParameterDTO deletePodcastBuddyReviewParameterDTO, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var existingReview = await _podcastBuddyReviewGenericRepository.FindByIdAsync(deletePodcastBuddyReviewParameterDTO.PodcastBuddyReviewId);
                    if (existingReview == null)
                    {
                        throw new Exception("Podcast buddy review not found");
                    }
                    else if (existingReview.AccountId != deletePodcastBuddyReviewParameterDTO.AccountId)
                    {
                        throw new Exception("Account with id " + deletePodcastBuddyReviewParameterDTO.AccountId + " is not the owner of podcast buddy review with id " + deletePodcastBuddyReviewParameterDTO.PodcastBuddyReviewId);
                    }
                    await _podcastBuddyReviewGenericRepository.DeleteAsync(existingReview.Id);

                    var podcasterProfile = (await _podcasterProfileGenericRepository.FindAll(
                         predicate: a => a.AccountId == existingReview.PodcastBuddyId && a.IsVerified == true,
                         includeFunc: null
                         ).ToListAsync()).FirstOrDefault();
                    if (podcasterProfile == null)
                    {
                        throw new Exception("Podcast buddy with id " + existingReview.PodcastBuddyId + " does not exist");
                    }

                    // cập nhật rating count cho podcaster profile
                    podcasterProfile.RatingCount -= 1;
                    podcasterProfile.AverageRating = podcasterProfile.RatingCount == 0 ? 0 : ((podcasterProfile.AverageRating * (podcasterProfile.RatingCount + 1)) - existingReview.Rating) / podcasterProfile.RatingCount;
                    await _podcasterProfileGenericRepository.UpdateAsync(podcasterProfile.AccountId, podcasterProfile);

                    await transaction.CommitAsync();

                    var messageNextRequestData = command.RequestData;
                    messageNextRequestData["PodcastBuddyReviewId"] = existingReview.Id;
                    messageNextRequestData["AccountId"] = existingReview.AccountId;
                    var messageResponseData = JObject.FromObject(new
                    {
                        // Message = "Delete podcast buddy review successfully",
                        PodcastBuddyReviewId = existingReview.Id,
                        AccountId = existingReview.AccountId,
                    });
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.PublicReviewManagementDomain,
                        requestData: messageNextRequestData,
                        responseData: messageResponseData,
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "delete-podcast-buddy-review.success"
                    );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();

                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.PublicReviewManagementDomain,
                        requestData: command.RequestData,
                        responseData: JObject.FromObject(new
                        {
                            ErrorMessage = $"Delete podcast buddy review failed, error: {ex.Message}"
                        }),
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "delete-podcast-buddy-review.failed"
                    );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);
                    Console.WriteLine("\n" + ex.StackTrace + "\n");
                }
            }
        }


        public async Task CreatePodcasterFollowed(CreatePodcasterFollowedParameterDTO createPodcasterFollowedParameterDTO, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    // chỉ follow podcaster đã được verify và account podcaster không bị deactive
                    var podcasterProfile = (await _podcasterProfileGenericRepository.FindAll(
                         predicate: a => a.AccountId == createPodcasterFollowedParameterDTO.PodcastBuddyId && a.IsVerified == true,
                         includeFunc: null
                         ).ToListAsync()).FirstOrDefault();
                    if (podcasterProfile == null)
                    {
                        throw new Exception("Podcast buddy with id " + createPodcasterFollowedParameterDTO.PodcastBuddyId + " does not exist");
                    }

                    var existingPodcasterAccount = await _accountCachingService.GetAccountStatusCacheById(createPodcasterFollowedParameterDTO.PodcastBuddyId);
                    if (existingPodcasterAccount == null || existingPodcasterAccount.DeactivatedAt != null)
                    {
                        throw new Exception("Podcast buddy account with id " + createPodcasterFollowedParameterDTO.PodcastBuddyId + " does not exist or is deactivated");
                    }


                    var existingFollow = (await _accountFollowedPodcasterGenericRepository.FindAll(
                        predicate: a => a.AccountId == createPodcasterFollowedParameterDTO.AccountId && a.PodcasterId == createPodcasterFollowedParameterDTO.PodcastBuddyId,
                        includeFunc: null
                        ).ToListAsync()).FirstOrDefault();
                    if (existingFollow != null)
                    {
                        throw new Exception("Account with id " + createPodcasterFollowedParameterDTO.AccountId + " has already followed podcast buddy with id " + createPodcasterFollowedParameterDTO.PodcastBuddyId);
                    }

                    var podcasterFollowed = new AccountFollowedPodcaster
                    {
                        AccountId = createPodcasterFollowedParameterDTO.AccountId,
                        PodcasterId = createPodcasterFollowedParameterDTO.PodcastBuddyId,
                    };

                    await _accountFollowedPodcasterGenericRepository.CreateAsync(podcasterFollowed);

                    podcasterProfile.TotalFollow += 1;
                    await _podcasterProfileGenericRepository.UpdateAsync(podcasterProfile.AccountId, podcasterProfile);

                    await transaction.CommitAsync();

                    var messageNextRequestData = command.RequestData;
                    messageNextRequestData["AccountId"] = podcasterFollowed.AccountId;
                    messageNextRequestData["PodcastBuddyId"] = podcasterFollowed.PodcasterId;
                    var messageResponseData = JObject.FromObject(new
                    {
                        // Message = "Create podcaster followed successfully",
                        FollowedPodcasterId = podcasterFollowed.PodcasterId,
                        AccountId = podcasterFollowed.AccountId,
                    });
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.UserManagementDomain,
                        requestData: messageNextRequestData,
                        responseData: messageResponseData,
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "create-podcaster-followed.success"
                        );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);

                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();

                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.UserManagementDomain,
                        requestData: command.RequestData,
                        responseData: JObject.FromObject(new
                        {
                            ErrorMessage = $"Create podcaster followed failed, error: {ex.Message}"
                        }),
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "create-podcaster-followed.failed"
                        );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);
                    Console.WriteLine("\n" + ex.StackTrace + "\n");
                }
            }
        }

        public async Task DeletePodcasterFollowed(DeletePodcasterFollowedParameterDTO deletePodcasterFollowedParameterDTO, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var podcasterProfile = (await _podcasterProfileGenericRepository.FindAll(
                         predicate: a => a.AccountId == deletePodcasterFollowedParameterDTO.PodcastBuddyId && a.IsVerified == true,
                         includeFunc: null
                         ).ToListAsync()).FirstOrDefault();
                    if (podcasterProfile == null)
                    {
                        throw new Exception("Podcast buddy with id " + deletePodcasterFollowedParameterDTO.PodcastBuddyId + " does not exist");
                    }


                    List<int> affectedAccountIds = new List<int>();
                    if (deletePodcasterFollowedParameterDTO.AccountId == null)
                    {
                        // xoá tất cả 
                        affectedAccountIds = await _unitOfWork.AccountFollowedPodcasterRepository.DeleteByPodcasterIdAsync(deletePodcasterFollowedParameterDTO.PodcastBuddyId);
                    }
                    else
                    {
                        var existingFollow = await (_accountFollowedPodcasterGenericRepository.FindAll(
                            predicate: a => a.AccountId == deletePodcasterFollowedParameterDTO.AccountId && a.PodcasterId == deletePodcasterFollowedParameterDTO.PodcastBuddyId
                            )).FirstOrDefaultAsync();
                        if (existingFollow != null)
                        {
                            await _unitOfWork.AccountFollowedPodcasterRepository.DeleteByAccountIdAndPodcasterIdAsync(deletePodcasterFollowedParameterDTO.AccountId.Value, deletePodcasterFollowedParameterDTO.PodcastBuddyId);
                            affectedAccountIds.Add(deletePodcasterFollowedParameterDTO.AccountId.Value);
                        }

                    }

                    // var existingFollow = (await _unitOfWork.AccountFollowedPodcasterRepository.FindAll(
                    //     predicate: a => a.AccountId == deletePodcasterFollowedParameterDTO.AccountId && a.PodcasterId == deletePodcasterFollowedParameterDTO.PodcastBuddyId
                    //     )).FirstOrDefault();

                    // await _unitOfWork.AccountFollowedPodcasterRepository.DeleteByAccountIdAndPodcasterIdAsync(deletePodcasterFollowedParameterDTO.AccountId, deletePodcasterFollowedParameterDTO.PodcastBuddyId);

                    podcasterProfile.TotalFollow -= affectedAccountIds.Count;
                    await _podcasterProfileGenericRepository.UpdateAsync(podcasterProfile.AccountId, podcasterProfile);
                    await transaction.CommitAsync();

                    var messageNextRequestData = command.RequestData;
                    messageNextRequestData["AccountId"] = deletePodcasterFollowedParameterDTO.AccountId;
                    messageNextRequestData["PodcastBuddyId"] = deletePodcasterFollowedParameterDTO.PodcastBuddyId;
                    messageNextRequestData["AffectedAccountIds"] = JArray.FromObject(affectedAccountIds);
                    var messageResponseData = JObject.FromObject(new
                    {
                        // Message = "Delete podcaster followed successfully",
                        UnfollowedPodcasterId = deletePodcasterFollowedParameterDTO.PodcastBuddyId,
                        AccountId = deletePodcasterFollowedParameterDTO.AccountId,
                        AffectedAccountIds = affectedAccountIds
                    });
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.UserManagementDomain,
                        requestData: command.RequestData,
                        responseData: messageResponseData,
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "delete-podcaster-followed.success"
                    );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();

                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.UserManagementDomain,
                        requestData: command.RequestData,
                        responseData: JObject.FromObject(new
                        {
                            ErrorMessage = $"Delete podcaster followed failed, error: {ex.Message}"
                        }),
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "delete-podcaster-followed.failed"
                    );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);
                    Console.WriteLine("\n" + ex.StackTrace + "\n");
                }
            }
        }

        public async Task CreateChannelFavorited(CreateChannelFavoritedParameterDTO channelFavoritedParameterDTO, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {

                    var existingFavorite = (await _accountFavoritedPodcastChannelGenericRepository.FindAll(
                        predicate: a => a.AccountId == channelFavoritedParameterDTO.AccountId && a.PodcastChannelId == channelFavoritedParameterDTO.PodcastChannelId,
                        includeFunc: null
                        ).ToListAsync()).FirstOrDefault();
                    if (existingFavorite != null)
                    {
                        throw new Exception("Account with id " + channelFavoritedParameterDTO.AccountId + " has already favorited podcast channel with id " + channelFavoritedParameterDTO.PodcastChannelId);
                    }

                    var podcastChannelBatchRequest = new BatchQueryRequest
                    {
                        Queries = new List<BatchQueryItem>
                    {
                        new BatchQueryItem
                        {
                            Key = "podcastChannel",
                            QueryType = "findbyid",
                            EntityType = "PodcastChannel",
                            Parameters = JObject.FromObject(new
                            {
                                where =  new {
                                    DeletedAt = (DateTime?)null,
                                },
                                id = channelFavoritedParameterDTO.PodcastChannelId,
                                include = "PodcastChannelStatusTrackings"
                            }),
                        }
                    }
                    };

                    var result = await _httpServiceQueryClient.ExecuteBatchAsync("PodcastService", podcastChannelBatchRequest);


                    var podcastChannel = result.Results["podcastChannel"].ToObject<PodcastChannelDTO>();
                    if (podcastChannel == null)
                    {
                        throw new Exception("Podcast channel with id " + channelFavoritedParameterDTO.PodcastChannelId + " does not exist");
                    }
                    else if (podcastChannel.PodcasterId == channelFavoritedParameterDTO.AccountId)
                    {
                        throw new Exception("Podcaster cannot favorite their own podcast channel");
                    }

                    var latestChannelStatusTracking = podcastChannel.PodcastChannelStatusTrackings.OrderByDescending(pct => pct.CreatedAt).Select(pct => pct).FirstOrDefault();
                    if (latestChannelStatusTracking == null || latestChannelStatusTracking.PodcastChannelStatusId != (int)PodcastChannelStatusEnum.Published)
                    {
                        throw new Exception("Podcast channel with id " + channelFavoritedParameterDTO.PodcastChannelId + " is not published, cannot be favorited");
                    }

                    var channelFavorited = new AccountFavoritedPodcastChannel
                    {
                        AccountId = channelFavoritedParameterDTO.AccountId,
                        PodcastChannelId = channelFavoritedParameterDTO.PodcastChannelId,
                    };

                    await _accountFavoritedPodcastChannelGenericRepository.CreateAsync(channelFavorited);

                    await transaction.CommitAsync();

                    var messageNextRequestData = command.RequestData;
                    messageNextRequestData["AccountId"] = channelFavorited.AccountId;
                    messageNextRequestData["PodcastChannelId"] = channelFavorited.PodcastChannelId;
                    var messageResponseData = JObject.FromObject(new
                    {
                        // Message = "Create channel favorited successfully",
                        PodcastChannelId = channelFavorited.PodcastChannelId,
                        AccountId = channelFavorited.AccountId,
                    });
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.UserManagementDomain,
                        requestData: messageNextRequestData,
                        responseData: messageResponseData,
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "create-channel-favorited.success"
                        );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);

                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();

                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.UserManagementDomain,
                        requestData: command.RequestData,
                        responseData: JObject.FromObject(new
                        {
                            ErrorMessage = $"Create channel favorited failed, error: {ex.Message}"
                        }),
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "create-channel-favorited.failed"
                        );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);
                    Console.WriteLine("\n" + ex.StackTrace + "\n");
                }
            }
        }

        public async Task CreateChannelFavoritedRollback(CreateChannelFavoritedRollbackParameterDTO channelFavoritedRollbackParameterDTO, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var existingFavorite = (await _accountFavoritedPodcastChannelGenericRepository.FindAll(
                        predicate: a => a.AccountId == channelFavoritedRollbackParameterDTO.AccountId && a.PodcastChannelId == channelFavoritedRollbackParameterDTO.PodcastChannelId,
                        includeFunc: null
                        ).ToListAsync()).FirstOrDefault();
                    if (existingFavorite == null)
                    {
                        throw new Exception("Account with id " + channelFavoritedRollbackParameterDTO.AccountId + " has not favorited podcast channel with id " + channelFavoritedRollbackParameterDTO.PodcastChannelId);
                    }
                    await _unitOfWork.AccountFavoritedPodcastChannelRepository.DeleteByAccountIdAndPodcastChannelIdAsync(channelFavoritedRollbackParameterDTO.AccountId, channelFavoritedRollbackParameterDTO.PodcastChannelId);

                    await transaction.CommitAsync();

                    var messageNextRequestData = command.RequestData;
                    messageNextRequestData["AccountId"] = channelFavoritedRollbackParameterDTO.AccountId;
                    messageNextRequestData["PodcastChannelId"] = channelFavoritedRollbackParameterDTO.PodcastChannelId;
                    var messageResponseData = JObject.FromObject(new
                    {
                        // Message = "Create channel favorited rollback successfully",
                        PodcastChannelId = channelFavoritedRollbackParameterDTO.PodcastChannelId,
                        AccountId = channelFavoritedRollbackParameterDTO.AccountId,
                    });
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.UserManagementDomain,
                        requestData: messageNextRequestData,
                        responseData: messageResponseData,
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "create-channel-favorited-rollback.success"
                    );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);

                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.UserManagementDomain,
                        requestData: command.RequestData,
                        responseData: JObject.FromObject(new
                        {
                            ErrorMessage = $"Create channel favorited rollback failed, error: {ex.Message}"
                        }),
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "create-channel-favorited-rollback.failed"
                    );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);
                    Console.WriteLine("\n" + ex.StackTrace + "\n");
                }
            }
        }

        public async Task DeleteChannelFavorited(DeleteChannelFavoritedParameterDTO deleteChannelFavoritedParameterDTO, SagaCommandMessage sagaCommand)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {

                    List<int> affectedAccountIds = new List<int>();
                    if (deleteChannelFavoritedParameterDTO.AccountId == null)
                    {
                        affectedAccountIds = await _unitOfWork.AccountFavoritedPodcastChannelRepository.DeleteByPodcastChannelIdAsync(deleteChannelFavoritedParameterDTO.PodcastChannelId);
                    }
                    else
                    {
                        var existingFavorite = await (_accountFavoritedPodcastChannelGenericRepository.FindAll(
                            predicate: a => a.AccountId == deleteChannelFavoritedParameterDTO.AccountId && a.PodcastChannelId == deleteChannelFavoritedParameterDTO.PodcastChannelId
                            )).FirstOrDefaultAsync();
                        if (existingFavorite != null)
                        {
                            // throw new Exception("Account with id " + deleteChannelFavoritedParameterDTO.AccountId + " has not favorited podcast channel with id " + deleteChannelFavoritedParameterDTO.PodcastChannelId);
                            await _unitOfWork.AccountFavoritedPodcastChannelRepository.DeleteByAccountIdAndPodcastChannelIdAsync(deleteChannelFavoritedParameterDTO.AccountId ?? 1, deleteChannelFavoritedParameterDTO.PodcastChannelId);
                            affectedAccountIds = new List<int> { deleteChannelFavoritedParameterDTO.AccountId ?? 1 };
                        }

                    }

                    await transaction.CommitAsync();

                    var messageNextRequestData = sagaCommand.RequestData;
                    messageNextRequestData["AccountId"] = deleteChannelFavoritedParameterDTO.AccountId;
                    messageNextRequestData["PodcastChannelId"] = deleteChannelFavoritedParameterDTO.PodcastChannelId;
                    messageNextRequestData["AffectedAccountIds"] = JArray.FromObject(affectedAccountIds);
                    var messageResponseData = JObject.FromObject(new
                    {
                        // Message = "Delete channel favorited successfully",
                        PodcastChannelId = deleteChannelFavoritedParameterDTO.PodcastChannelId,
                        AccountId = deleteChannelFavoritedParameterDTO.AccountId,
                        AffectedAccountIds = affectedAccountIds,
                    });
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.UserManagementDomain,
                        requestData: messageNextRequestData,
                        responseData: messageResponseData,
                        sagaInstanceId: sagaCommand.SagaInstanceId,
                        flowName: sagaCommand.FlowName,
                        messageName: "delete-channel-favorited.success"
                    );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();

                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.UserManagementDomain,
                        requestData: sagaCommand.RequestData,
                        responseData: JObject.FromObject(new
                        {
                            ErrorMessage = $"Delete channel favorited failed, error: {ex.Message}"
                        }),
                        sagaInstanceId: sagaCommand.SagaInstanceId,
                        flowName: sagaCommand.FlowName,
                        messageName: "delete-channel-favorited.failed"
                    );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);
                    Console.WriteLine("\n" + ex.StackTrace + "\n");
                }
            }
        }

        public async Task DeleteChannelFavoritedRollback(DeleteChannelFavoritedRollbackParameterDTO deleteChannelFavoritedRollbackParameterDTO, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var existingFavorite = (await _accountFavoritedPodcastChannelGenericRepository.FindAll(
                         predicate: a => deleteChannelFavoritedRollbackParameterDTO.AffectedAccountIds.Contains(a.AccountId) && a.PodcastChannelId == deleteChannelFavoritedRollbackParameterDTO.PodcastChannelId,
                         includeFunc: null
                         ).ToListAsync());
                    // if (existingFavorite != null)
                    // {
                    //     throw new Exception("Account with id " + deleteChannelFavoritedRollbackParameterDTO.AccountId + " has already favorited podcast channel with id " + deleteChannelFavoritedRollbackParameterDTO.PodcastChannelId);
                    // }
                    if (existingFavorite != null && existingFavorite.Count > 0)
                    {
                        var existingAccountIds = existingFavorite.Select(ef => ef.AccountId).ToList();
                        deleteChannelFavoritedRollbackParameterDTO.AffectedAccountIds = deleteChannelFavoritedRollbackParameterDTO.AffectedAccountIds.Except(existingAccountIds).ToList(); // tránh tạo lại những favorite đã tồn tại
                    }

                    foreach (var accountId in deleteChannelFavoritedRollbackParameterDTO.AffectedAccountIds)
                    {
                        var channelFavorited = new AccountFavoritedPodcastChannel
                        {
                            AccountId = accountId,
                            PodcastChannelId = deleteChannelFavoritedRollbackParameterDTO.PodcastChannelId,
                        };
                        await _accountFavoritedPodcastChannelGenericRepository.CreateAsync(channelFavorited);
                    }

                    await transaction.CommitAsync();

                    var messageNextRequestData = command.RequestData;
                    messageNextRequestData["AffectedAccountIds"] = JArray.FromObject(deleteChannelFavoritedRollbackParameterDTO.AffectedAccountIds);
                    messageNextRequestData["PodcastChannelId"] = deleteChannelFavoritedRollbackParameterDTO.PodcastChannelId;
                    var messageResponseData = JObject.FromObject(new
                    {
                        // Message = "Delete channel favorited rollback successfully",
                        PodcastChannelId = deleteChannelFavoritedRollbackParameterDTO.PodcastChannelId,
                        AffectedAccountIds = deleteChannelFavoritedRollbackParameterDTO.AffectedAccountIds,
                    });
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.UserManagementDomain,
                        requestData: messageNextRequestData,
                        responseData: messageResponseData,
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "delete-channel-favorited-rollback.success"
                    );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);

                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.UserManagementDomain,
                        requestData: command.RequestData,
                        responseData: JObject.FromObject(new
                        {
                            ErrorMessage = $"Delete channel favorited rollback failed, error: {ex.Message}"
                        }),
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "delete-channel-favorited-rollback.failed"
                    );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);
                    Console.WriteLine("\n" + ex.StackTrace + "\n");
                }
            }
        }

        public async Task CreateShowFollowed(CreateShowFollowedParameterDTO createShowFollowedParameterDTO, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {

                    var existingFollow = (await _accountFollowedPodcastShowGenericRepository.FindAll(
                        predicate: a => a.AccountId == createShowFollowedParameterDTO.AccountId && a.PodcastShowId == createShowFollowedParameterDTO.PodcastShowId,
                        includeFunc: null
                        ).ToListAsync()).FirstOrDefault();
                    if (existingFollow != null)
                    {
                        throw new Exception("Account with id " + createShowFollowedParameterDTO.AccountId + " has already followed podcast show with id " + createShowFollowedParameterDTO.PodcastShowId);
                    }

                    var podcastChannelBatchRequest = new BatchQueryRequest
                    {
                        Queries = new List<BatchQueryItem>
                    {
                        new BatchQueryItem
                        {
                            Key = "podcastShow",
                            QueryType = "findbyid",
                            EntityType = "PodcastShow",
                            Parameters = JObject.FromObject(new
                            {
                                where =  new {
                                    DeletedAt = (DateTime?)null,
                                },
                                id = createShowFollowedParameterDTO.PodcastShowId,
                                include = "PodcastShowStatusTrackings, PodcastChannel, PodcastChannel.PodcastChannelStatusTrackings"
                            }),
                        }
                    }
                    };

                    var result = await _httpServiceQueryClient.ExecuteBatchAsync("PodcastService", podcastChannelBatchRequest);


                    var podcastShow = result.Results["podcastShow"].ToObject<PodcastShowDTO>();
                    if (podcastShow == null)
                    {
                        throw new Exception("Podcast show with id " + createShowFollowedParameterDTO.PodcastShowId + " does not exist");
                    }
                    else if (podcastShow.PodcasterId == createShowFollowedParameterDTO.AccountId)
                    {
                        throw new Exception("Podcaster cannot favorite their own podcast show");
                    }


                    // Nếu show thuộc về channel chưa được publish hoặc đã bị delete thì không thể favorite
                    if (podcastShow.PodcastChannel != null)
                    {
                        var latestChannelStatusTracking = podcastShow.PodcastChannel.PodcastChannelStatusTrackings.OrderByDescending(pct => pct.CreatedAt).Select(pct => pct).FirstOrDefault();
                        if (latestChannelStatusTracking == null || latestChannelStatusTracking.PodcastChannelStatusId != (int)PodcastChannelStatusEnum.Published || podcastShow.PodcastChannel.DeletedAt != null)
                        {
                            throw new Exception("Podcast channel with id " + podcastShow.PodcastChannelId + " is not published or deleted, cannot follow its shows");
                        }
                    }

                    var latestShowStatusTracking = podcastShow.PodcastShowStatusTrackings.OrderByDescending(pct => pct.CreatedAt).Select(pct => pct).FirstOrDefault();
                    if (latestShowStatusTracking == null || latestShowStatusTracking.PodcastShowStatusId != (int)PodcastShowStatusEnum.Published)
                    {
                        throw new Exception("Podcast show with id " + createShowFollowedParameterDTO.PodcastShowId + " is not published, cannot be followed");
                    }

                    var showFavorited = new AccountFollowedPodcastShow
                    {
                        AccountId = createShowFollowedParameterDTO.AccountId,
                        PodcastShowId = createShowFollowedParameterDTO.PodcastShowId,
                    };

                    await _accountFollowedPodcastShowGenericRepository.CreateAsync(showFavorited);

                    await transaction.CommitAsync();

                    var messageNextRequestData = command.RequestData;
                    messageNextRequestData["AccountId"] = showFavorited.AccountId;
                    messageNextRequestData["PodcastShowId"] = showFavorited.PodcastShowId;
                    var messageResponseData = JObject.FromObject(new
                    {
                        // Message = "Create channel favorited successfully",
                        PodcastShowId = showFavorited.PodcastShowId,
                        AccountId = showFavorited.AccountId,
                    });
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.UserManagementDomain,
                        requestData: messageNextRequestData,
                        responseData: messageResponseData,
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "create-show-followed.success"
                        );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);

                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();

                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.UserManagementDomain,
                        requestData: command.RequestData,
                        responseData: JObject.FromObject(new
                        {
                            ErrorMessage = $"Create show followed failed, error: {ex.Message}"
                        }),
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "create-show-followed.failed"
                        );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);
                    Console.WriteLine("\n" + ex.StackTrace + "\n");
                }
            }
        }

        public async Task CreateShowFollowedRollback(CreateShowFollowedRollbackParameterDTO createShowFollowedRollbackParameterDTO, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var existingFollow = (await _accountFollowedPodcastShowGenericRepository.FindAll(
                         predicate: a => a.AccountId == createShowFollowedRollbackParameterDTO.AccountId && a.PodcastShowId == createShowFollowedRollbackParameterDTO.PodcastShowId,
                         includeFunc: null
                         ).ToListAsync()).FirstOrDefault();
                    if (existingFollow == null)
                    {
                        throw new Exception("Account with id " + createShowFollowedRollbackParameterDTO.AccountId + " has not followed podcast show with id " + createShowFollowedRollbackParameterDTO.PodcastShowId);
                    }
                    await _unitOfWork.AccountFollowedPodcastShowRepository.DeleteByAccountIdAndPodcastShowIdAsync(createShowFollowedRollbackParameterDTO.AccountId, createShowFollowedRollbackParameterDTO.PodcastShowId);

                    await transaction.CommitAsync();

                    var messageNextRequestData = command.RequestData;
                    messageNextRequestData["AccountId"] = createShowFollowedRollbackParameterDTO.AccountId;
                    messageNextRequestData["PodcastShowId"] = createShowFollowedRollbackParameterDTO.PodcastShowId;
                    var messageResponseData = JObject.FromObject(new
                    {
                        // Message = "Create show followed rollback successfully",
                        PodcastShowId = createShowFollowedRollbackParameterDTO.PodcastShowId,
                        AccountId = createShowFollowedRollbackParameterDTO.AccountId,
                    });
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.UserManagementDomain,
                        requestData: messageNextRequestData,
                        responseData: messageResponseData,
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "create-show-followed-rollback.success"
                    );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);

                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.UserManagementDomain,
                        requestData: command.RequestData,
                        responseData: JObject.FromObject(new
                        {
                            ErrorMessage = $"Create show followed rollback failed, error: {ex.Message}"
                        }),
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "create-show-followed-rollback.failed"
                    );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);
                    Console.WriteLine("\n" + ex.StackTrace + "\n");
                }
            }
        }

        public async Task DeleteShowFollowed(DeleteShowFollowedParameterDTO deleteShowFollowedParameterDTO, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {

                    List<int> affectedAccountIds = new List<int>();
                    if (deleteShowFollowedParameterDTO.AccountId == null)
                    {
                        affectedAccountIds = await _unitOfWork.AccountFollowedPodcastShowRepository.DeleteByPodcastShowIdAsync(deleteShowFollowedParameterDTO.PodcastShowId);
                    }
                    else
                    {
                        var existingFollow = await (_accountFollowedPodcastShowGenericRepository.FindAll(
                            predicate: a => a.AccountId == deleteShowFollowedParameterDTO.AccountId && a.PodcastShowId == deleteShowFollowedParameterDTO.PodcastShowId
                            )).FirstOrDefaultAsync();
                        if (existingFollow != null)
                        {
                            // throw new Exception("Account with id " + deleteShowFollowedParameterDTO.AccountId + " has not followed podcast show with id " + deleteShowFollowedParameterDTO.PodcastShowId);
                            await _unitOfWork.AccountFollowedPodcastShowRepository.DeleteByAccountIdAndPodcastShowIdAsync(deleteShowFollowedParameterDTO.AccountId ?? 1, deleteShowFollowedParameterDTO.PodcastShowId);
                            affectedAccountIds = new List<int> { deleteShowFollowedParameterDTO.AccountId ?? 1 };
                        }

                    }


                    await transaction.CommitAsync();

                    var messageNextRequestData = command.RequestData;
                    messageNextRequestData["AccountId"] = deleteShowFollowedParameterDTO.AccountId;
                    messageNextRequestData["PodcastShowId"] = deleteShowFollowedParameterDTO.PodcastShowId;
                    messageNextRequestData["AffectedAccountIds"] = JArray.FromObject(affectedAccountIds);
                    var messageResponseData = JObject.FromObject(new
                    {
                        // Message = "Delete show followed successfully",
                        PodcastShowId = deleteShowFollowedParameterDTO.PodcastShowId,
                        AccountId = deleteShowFollowedParameterDTO.AccountId,
                        AffectedAccountIds = affectedAccountIds,
                    });
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.UserManagementDomain,
                        requestData: messageNextRequestData,
                        responseData: messageResponseData,
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "delete-show-followed.success"
                    );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();

                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.UserManagementDomain,
                        requestData: command.RequestData,
                        responseData: JObject.FromObject(new
                        {
                            ErrorMessage = $"Delete show followed failed, error: {ex.Message}"
                        }),
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "delete-show-followed.failed"
                    );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);
                    Console.WriteLine("\n" + ex.StackTrace + "\n");
                }
            }
        }

        public async Task DeleteShowFollowedRollback(DeleteShowFollowedRollbackParameterDTO deleteShowFollowedRollbackParameterDTO, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var existingFollow = (await _accountFollowedPodcastShowGenericRepository.FindAll(
                         predicate: a => deleteShowFollowedRollbackParameterDTO.AffectedAccountIds.Contains(a.AccountId) && a.PodcastShowId == deleteShowFollowedRollbackParameterDTO.PodcastShowId,
                         includeFunc: null
                         ).ToListAsync());


                    if (existingFollow != null && existingFollow.Count > 0)
                    {
                        var existingAccountIds = existingFollow.Select(ef => ef.AccountId).ToList();
                        deleteShowFollowedRollbackParameterDTO.AffectedAccountIds = deleteShowFollowedRollbackParameterDTO.AffectedAccountIds.Except(existingAccountIds).ToList(); // tránh tạo lại những favorite đã tồn tại
                    }

                    foreach (var accountId in deleteShowFollowedRollbackParameterDTO.AffectedAccountIds)
                    {
                        var showFollowed = new AccountFollowedPodcastShow
                        {
                            AccountId = accountId,
                            PodcastShowId = deleteShowFollowedRollbackParameterDTO.PodcastShowId,
                        };
                        await _accountFollowedPodcastShowGenericRepository.CreateAsync(showFollowed);
                    }

                    await transaction.CommitAsync();

                    var messageNextRequestData = command.RequestData;
                    messageNextRequestData["AffectedAccountIds"] = JArray.FromObject(deleteShowFollowedRollbackParameterDTO.AffectedAccountIds);
                    messageNextRequestData["PodcastShowId"] = deleteShowFollowedRollbackParameterDTO.PodcastShowId;
                    var messageResponseData = JObject.FromObject(new
                    {
                        // Message = "Delete show followed rollback successfully",
                        PodcastShowId = deleteShowFollowedRollbackParameterDTO.PodcastShowId,
                        AffectedAccountIds = deleteShowFollowedRollbackParameterDTO.AffectedAccountIds,
                    });
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.UserManagementDomain,
                        requestData: messageNextRequestData,
                        responseData: messageResponseData,
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "delete-show-followed-rollback.success"
                    );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);

                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.UserManagementDomain,
                        requestData: command.RequestData,
                        responseData: JObject.FromObject(new
                        {
                            ErrorMessage = $"Delete show followed rollback failed, error: {ex.Message}"
                        }),
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "delete-show-followed-rollback.failed"
                    );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);
                    Console.WriteLine("\n" + ex.StackTrace + "\n");
                }
            }
        }

        public async Task CreateEpisodeSaved(CreateEpisodeSavedParameterDTO createEpisodeSavedParameterDTO, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {

                    var existingSave = (await _accountSavedPodcastEpisodeGenericRepository.FindAll(
                        predicate: a => a.AccountId == createEpisodeSavedParameterDTO.AccountId && a.PodcastEpisodeId == createEpisodeSavedParameterDTO.PodcastEpisodeId,
                        includeFunc: null
                        ).ToListAsync()).FirstOrDefault();
                    if (existingSave != null)
                    {
                        throw new Exception("Account with id " + createEpisodeSavedParameterDTO.AccountId + " has already saved podcast episode with id " + createEpisodeSavedParameterDTO.PodcastEpisodeId);
                    }

                    var podcastChannelBatchRequest = new BatchQueryRequest
                    {
                        Queries = new List<BatchQueryItem>
                    {
                        new BatchQueryItem
                        {
                            Key = "podcastEpisode",
                            QueryType = "findbyid",
                            EntityType = "PodcastEpisode",
                            Parameters = JObject.FromObject(new
                            {
                                where =  new {
                                    DeletedAt = (DateTime?)null,
                                },
                                id = createEpisodeSavedParameterDTO.PodcastEpisodeId,
                                include = "PodcastEpisodeStatusTrackings, PodcastShow, PodcastShow.PodcastShowStatusTrackings, PodcastShow.PodcastChannel, PodcastShow.PodcastChannel.PodcastChannelStatusTrackings"
                            }),
                        }
                    }
                    };

                    var result = await _httpServiceQueryClient.ExecuteBatchAsync("PodcastService", podcastChannelBatchRequest);


                    var podcastEpisode = result.Results["podcastEpisode"].ToObject<PodcastEpisodeDTO>();
                    if (podcastEpisode == null)
                    {
                        throw new Exception("Podcast episode with id " + createEpisodeSavedParameterDTO.PodcastEpisodeId + " does not exist");
                    }
                    else if (podcastEpisode.PodcastShow.DeletedAt != null)
                    {
                        throw new Exception("Podcast episode with id " + createEpisodeSavedParameterDTO.PodcastEpisodeId + " does not exist");
                    }
                    else if (podcastEpisode.PodcastShow.PodcasterId == createEpisodeSavedParameterDTO.AccountId)
                    {
                        throw new Exception("Podcaster cannot favorite their own podcast show");
                    }

                    // Nếu show thuộc về channel chưa được publish hoặc đã bị delete thì không thể save
                    if (podcastEpisode.PodcastShow.PodcastChannel != null)
                    {
                        var latestChannelStatusTracking = podcastEpisode.PodcastShow.PodcastChannel.PodcastChannelStatusTrackings.OrderByDescending(pct => pct.CreatedAt).Select(pct => pct).FirstOrDefault();
                        if (latestChannelStatusTracking == null || latestChannelStatusTracking.PodcastChannelStatusId != (int)PodcastChannelStatusEnum.Published || podcastEpisode.PodcastShow.PodcastChannel.DeletedAt != null)
                        {
                            throw new Exception("Podcast channel with id " + podcastEpisode.PodcastShow.PodcastChannelId + " is not published or deleted, cannot save its shows' episodes");
                        }
                    }

                    var latestShowStatusTracking = podcastEpisode.PodcastShow.PodcastShowStatusTrackings.OrderByDescending(pct => pct.CreatedAt).Select(pct => pct).FirstOrDefault();
                    if (latestShowStatusTracking == null || latestShowStatusTracking.PodcastShowStatusId != (int)PodcastShowStatusEnum.Published || podcastEpisode.PodcastShow.DeletedAt != null)
                    {
                        throw new Exception("Podcast show with id " + podcastEpisode.PodcastShowId + " is not published or deleted, cannot save its episodes");
                    }

                    var latestEpisodeStatusTracking = podcastEpisode.PodcastEpisodeStatusTrackings.OrderByDescending(pct => pct.CreatedAt).Select(pct => pct).FirstOrDefault();
                    if (latestEpisodeStatusTracking == null || latestEpisodeStatusTracking.PodcastEpisodeStatusId != (int)PodcastEpisodeStatusEnum.Published)
                    {
                        throw new Exception("Podcast episode with id " + createEpisodeSavedParameterDTO.PodcastEpisodeId + " is not published, cannot be saved");
                    }

                    var episodeSaved = new AccountSavedPodcastEpisode
                    {
                        AccountId = createEpisodeSavedParameterDTO.AccountId,
                        PodcastEpisodeId = createEpisodeSavedParameterDTO.PodcastEpisodeId,
                    };

                    await _accountSavedPodcastEpisodeGenericRepository.CreateAsync(episodeSaved);

                    await transaction.CommitAsync();

                    var messageNextRequestData = command.RequestData;
                    messageNextRequestData["AccountId"] = episodeSaved.AccountId;
                    messageNextRequestData["PodcastEpisodeId"] = episodeSaved.PodcastEpisodeId;
                    var messageResponseData = JObject.FromObject(new
                    {
                        // Message = "Create channel favorited successfully",
                        PodcastEpisodeId = episodeSaved.PodcastEpisodeId,
                        AccountId = episodeSaved.AccountId,
                    });
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.UserManagementDomain,
                        requestData: messageNextRequestData,
                        responseData: messageResponseData,
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "create-episode-saved.success"
                        );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);

                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();

                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.UserManagementDomain,
                        requestData: command.RequestData,
                        responseData: JObject.FromObject(new
                        {
                            ErrorMessage = $"Create episode saved failed, error: {ex.Message}"
                        }),
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "create-episode-saved.failed"
                        );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);
                    Console.WriteLine("\n" + ex.StackTrace + "\n");
                }
            }
        }

        public async Task CreateEpisodeSavedRollback(CreateEpisodeSavedRollbackParameterDTO createEpisodeSavedRollbackParameterDTO, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var existingSave = (await _accountSavedPodcastEpisodeGenericRepository.FindAll(
                         predicate: a => a.AccountId == createEpisodeSavedRollbackParameterDTO.AccountId && a.PodcastEpisodeId == createEpisodeSavedRollbackParameterDTO.PodcastEpisodeId,
                         includeFunc: null
                         ).ToListAsync()).FirstOrDefault();
                    if (existingSave == null)
                    {
                        throw new Exception("Account with id " + createEpisodeSavedRollbackParameterDTO.AccountId + " has not saved podcast episode with id " + createEpisodeSavedRollbackParameterDTO.PodcastEpisodeId);
                    }
                    await _unitOfWork.AccountSavedPodcastEpisodeRepository.DeleteByAccountIdAndPodcastEpisodeIdAsync(createEpisodeSavedRollbackParameterDTO.AccountId, createEpisodeSavedRollbackParameterDTO.PodcastEpisodeId);

                    await transaction.CommitAsync();

                    var messageNextRequestData = command.RequestData;
                    messageNextRequestData["AccountId"] = createEpisodeSavedRollbackParameterDTO.AccountId;
                    messageNextRequestData["PodcastEpisodeId"] = createEpisodeSavedRollbackParameterDTO.PodcastEpisodeId;
                    var messageResponseData = JObject.FromObject(new
                    {
                        // Message = "Create episode saved rollback successfully",
                        PodcastEpisodeId = createEpisodeSavedRollbackParameterDTO.PodcastEpisodeId,
                        AccountId = createEpisodeSavedRollbackParameterDTO.AccountId,
                    });
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.UserManagementDomain,
                        requestData: messageNextRequestData,
                        responseData: messageResponseData,
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "create-episode-saved-rollback.success"
                    );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);

                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.UserManagementDomain,
                        requestData: command.RequestData,
                        responseData: JObject.FromObject(new
                        {
                            ErrorMessage = $"Create episode saved rollback failed, error: {ex.Message}"
                        }),
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "create-episode-saved-rollback.failed"
                    );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);
                    Console.WriteLine("\n" + ex.StackTrace + "\n");
                }
            }
        }

        public async Task DeleteEpisodeSaved(DeleteEpisodeSavedParameterDTO deleteEpisodeSavedParameterDTO, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {

                    List<int> affectedAccountIds = new List<int>();
                    if (deleteEpisodeSavedParameterDTO.AccountId == null)
                    {
                        affectedAccountIds = await _unitOfWork.AccountSavedPodcastEpisodeRepository.DeleteByPodcastEpisodeIdAsync(deleteEpisodeSavedParameterDTO.PodcastEpisodeId);
                    }
                    else
                    {
                        var existingSavedEpisode = await (_accountSavedPodcastEpisodeGenericRepository.FindAll(
                            predicate: a => a.AccountId == deleteEpisodeSavedParameterDTO.AccountId && a.PodcastEpisodeId == deleteEpisodeSavedParameterDTO.PodcastEpisodeId
                            )).FirstOrDefaultAsync();
                        if (existingSavedEpisode != null)
                        {
                            await _unitOfWork.AccountSavedPodcastEpisodeRepository.DeleteByAccountIdAndPodcastEpisodeIdAsync(deleteEpisodeSavedParameterDTO.AccountId ?? 1, deleteEpisodeSavedParameterDTO.PodcastEpisodeId);
                            affectedAccountIds = new List<int> { deleteEpisodeSavedParameterDTO.AccountId ?? 1 };
                        }

                    }


                    await transaction.CommitAsync();

                    var messageNextRequestData = command.RequestData;
                    messageNextRequestData["AccountId"] = deleteEpisodeSavedParameterDTO.AccountId;
                    messageNextRequestData["PodcastEpisodeId"] = deleteEpisodeSavedParameterDTO.PodcastEpisodeId;
                    messageNextRequestData["AffectedAccountIds"] = JArray.FromObject(affectedAccountIds);
                    var messageResponseData = JObject.FromObject(new
                    {
                        // Message = "Delete episode saved successfully",
                        PodcastEpisodeId = deleteEpisodeSavedParameterDTO.PodcastEpisodeId,
                        AccountId = deleteEpisodeSavedParameterDTO.AccountId,
                        AffectedAccountIds = affectedAccountIds,
                    });
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.UserManagementDomain,
                        requestData: messageNextRequestData,
                        responseData: messageResponseData,
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "delete-episode-saved.success"
                    );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();

                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.UserManagementDomain,
                        requestData: command.RequestData,
                        responseData: JObject.FromObject(new
                        {
                            ErrorMessage = $"Delete episode saved failed, error: {ex.Message}"
                        }),
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "delete-episode-saved.failed"
                    );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);
                    Console.WriteLine("\n" + ex.StackTrace + "\n");
                }
            }
        }

        public async Task DeleteEpisodeSavedRollback(DeleteEpisodeSavedRollbackParameterDTO deleteEpisodeSavedRollbackParameterDTO, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var existingSaves = (await _accountSavedPodcastEpisodeGenericRepository.FindAll(
                         predicate: a => deleteEpisodeSavedRollbackParameterDTO.AffectedAccountIds.Contains(a.AccountId) && a.PodcastEpisodeId == deleteEpisodeSavedRollbackParameterDTO.PodcastEpisodeId,
                         includeFunc: null
                         ).ToListAsync());
                    if (existingSaves != null && existingSaves.Count > 0)
                    {
                        var existingAccountIds = existingSaves.Select(ef => ef.AccountId).ToList();
                        deleteEpisodeSavedRollbackParameterDTO.AffectedAccountIds = deleteEpisodeSavedRollbackParameterDTO.AffectedAccountIds.Except(existingAccountIds).ToList(); // tránh tạo lại những favorite đã tồn tại
                    }
                    foreach (var accountId in deleteEpisodeSavedRollbackParameterDTO.AffectedAccountIds)
                    {
                        var episodeSaved = new AccountSavedPodcastEpisode
                        {
                            AccountId = accountId,
                            PodcastEpisodeId = deleteEpisodeSavedRollbackParameterDTO.PodcastEpisodeId,
                        };
                        await _accountSavedPodcastEpisodeGenericRepository.CreateAsync(episodeSaved);
                    }
                    await transaction.CommitAsync();
                    var messageNextRequestData = command.RequestData;
                    messageNextRequestData["AffectedAccountIds"] = JArray.FromObject(deleteEpisodeSavedRollbackParameterDTO.AffectedAccountIds);
                    messageNextRequestData["PodcastEpisodeId"] = deleteEpisodeSavedRollbackParameterDTO.PodcastEpisodeId;
                    var messageResponseData = JObject.FromObject(new
                    {
                        // Message = "Delete episode saved rollback successfully",
                        PodcastEpisodeId = deleteEpisodeSavedRollbackParameterDTO.PodcastEpisodeId,
                        AffectedAccountIds = deleteEpisodeSavedRollbackParameterDTO.AffectedAccountIds,
                    });
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.UserManagementDomain,
                        requestData: messageNextRequestData,
                        responseData: messageResponseData,
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "delete-episode-saved-rollback.success"
                    );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.UserManagementDomain,
                        requestData: command.RequestData,
                        responseData: JObject.FromObject(new
                        {
                            ErrorMessage = $"Delete episode saved rollback failed, error: {ex.Message}"
                        }),
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "delete-episode-saved-rollback.failed"
                    );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);
                    Console.WriteLine("\n" + ex.StackTrace + "\n");
                }
            }
        }

        public async Task AddAccountBalanceAmount(AddAccountBalanceAmountParameterDTO addAccountBalanceAmountParameterDTO, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    if (addAccountBalanceAmountParameterDTO.Amount < 0)
                    {
                        throw new Exception("Amount to add must be greater than or equal to 0");
                    }

                    var account = await _accountGenericRepository.FindByIdAsync(addAccountBalanceAmountParameterDTO.AccountId);
                    if (account == null)
                    {
                        throw new Exception("Account with id " + addAccountBalanceAmountParameterDTO.AccountId + " does not exist");
                    }
                    else if (account.IsVerified == false)
                    {
                        throw new Exception("Account with id " + addAccountBalanceAmountParameterDTO.AccountId + " is not verified");
                    }


                    account.Balance += addAccountBalanceAmountParameterDTO.Amount;
                    await _accountGenericRepository.UpdateAsync(account.Id, account);

                    await transaction.CommitAsync();

                    var messageNextRequestData = command.RequestData;
                    messageNextRequestData["AccountId"] = addAccountBalanceAmountParameterDTO.AccountId;
                    messageNextRequestData["Amount"] = addAccountBalanceAmountParameterDTO.Amount;

                    var messageResponseData = JObject.FromObject(new
                    {
                        // Message = "Add account balance amount successfully",
                        AccountId = addAccountBalanceAmountParameterDTO.AccountId,
                        Amount = addAccountBalanceAmountParameterDTO.Amount,
                        NewBalance = account.Balance,
                    });
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.UserManagementDomain,
                        requestData: messageNextRequestData,
                        responseData: messageResponseData,
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "add-account-balance-amount.success"
                        );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);

                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();

                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.UserManagementDomain,
                        requestData: command.RequestData,
                        responseData: JObject.FromObject(new
                        {
                            ErrorMessage = $"Add account balance amount failed, error: {ex.Message}"
                        }),
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "add-account-balance-amount.failed"
                        );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);
                    Console.WriteLine("\n" + ex.StackTrace + "\n");
                }
            }
        }

        public async Task AddAccountBalanceAmountRollback(AddAccountBalanceAmountRollbackParameterDTO addAccountBalanceAmountRollbackParameterDTO, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    if (addAccountBalanceAmountRollbackParameterDTO.Amount < 0)
                    {
                        throw new Exception("Amount to subtract must be greater than or equal to 0");
                    }

                    var account = await _accountGenericRepository.FindByIdAsync(addAccountBalanceAmountRollbackParameterDTO.AccountId);
                    if (account == null)
                    {
                        throw new Exception("Account with id " + addAccountBalanceAmountRollbackParameterDTO.AccountId + " does not exist");
                    }
                    else if (account.IsVerified == false)
                    {
                        throw new Exception("Account with id " + addAccountBalanceAmountRollbackParameterDTO.AccountId + " is not verified");
                    }

                    var isNegativeBalance = false;
                    account.Balance -= addAccountBalanceAmountRollbackParameterDTO.Amount;
                    if (account.Balance < 0)
                    {
                        account.Balance = 0;
                        isNegativeBalance = true;
                    }


                    await _accountGenericRepository.UpdateAsync(account.Id, account);

                    if (isNegativeBalance == true)
                    {
                        throw new Exception("Account balance cannot be negative, account balance is set to 0 instead of subsctract result of " + addAccountBalanceAmountRollbackParameterDTO.Amount);
                    }

                    await transaction.CommitAsync();

                    var messageNextRequestData = command.RequestData;
                    messageNextRequestData["AccountId"] = addAccountBalanceAmountRollbackParameterDTO.AccountId;
                    messageNextRequestData["Amount"] = addAccountBalanceAmountRollbackParameterDTO.Amount;

                    var messageResponseData = JObject.FromObject(new
                    {
                        // Message = "Subtract account balance amount successfully",
                        AccountId = addAccountBalanceAmountRollbackParameterDTO.AccountId,
                        Amount = addAccountBalanceAmountRollbackParameterDTO.Amount,
                        NewBalance = account.Balance,
                    });
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.UserManagementDomain,
                        requestData: messageNextRequestData,
                        responseData: messageResponseData,
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "add-account-balance-amount-rollback.success"
                        );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);

                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();

                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.UserManagementDomain,
                        requestData: command.RequestData,
                        responseData: JObject.FromObject(new
                        {
                            ErrorMessage = $"Add account balance amount rollback failed, error: {ex.Message}"
                        }),
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "add-account-balance-amount-rollback.failed"
                        );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);
                    Console.WriteLine("\n" + ex.StackTrace + "\n");
                }
            }
        }

        public async Task SubtractAccountBalanceAmount(SubtractAccountBalanceAmountParameterDTO subtractAccountBalanceAmountParameterDTO, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    if (subtractAccountBalanceAmountParameterDTO.Amount < 0)
                    {
                        throw new Exception("Amount to subtract must be greater than or equal to 0");
                    }

                    var account = await _accountGenericRepository.FindByIdAsync(subtractAccountBalanceAmountParameterDTO.AccountId);
                    if (account == null)
                    {
                        throw new Exception("Account with id " + subtractAccountBalanceAmountParameterDTO.AccountId + " does not exist");
                    }
                    else if (account.IsVerified == false)
                    {
                        throw new Exception("Account with id " + subtractAccountBalanceAmountParameterDTO.AccountId + " is not verified");
                    }

                    var isNegativeBalance = false;
                    account.Balance -= subtractAccountBalanceAmountParameterDTO.Amount;
                    if (account.Balance < 0)
                    {
                        account.Balance = 0;
                        isNegativeBalance = true;
                    }


                    await _accountGenericRepository.UpdateAsync(account.Id, account);

                    if (isNegativeBalance == true)
                    {
                        throw new Exception("Account balance cannot be negative, account balance is set to 0 instead of subsctract result of " + subtractAccountBalanceAmountParameterDTO.Amount);
                    }

                    await transaction.CommitAsync();

                    var messageNextRequestData = command.RequestData;
                    messageNextRequestData["AccountId"] = subtractAccountBalanceAmountParameterDTO.AccountId;
                    messageNextRequestData["Amount"] = subtractAccountBalanceAmountParameterDTO.Amount;

                    var messageResponseData = JObject.FromObject(new
                    {
                        // Message = "Subtract account balance amount successfully",
                        AccountId = subtractAccountBalanceAmountParameterDTO.AccountId,
                        Amount = subtractAccountBalanceAmountParameterDTO.Amount,
                        NewBalance = account.Balance,
                    });
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.UserManagementDomain,
                        requestData: messageNextRequestData,
                        responseData: messageResponseData,
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "subtract-account-balance-amount.success"
                        );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);

                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();

                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.UserManagementDomain,
                        requestData: command.RequestData,
                        responseData: JObject.FromObject(new
                        {
                            ErrorMessage = $"Subtract account balance amount failed, error: {ex.Message}"
                        }),
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "subtract-account-balance-amount.failed"
                        );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);
                    Console.WriteLine("\n" + ex.StackTrace + "\n");
                }
            }
        }

        public async Task SubtractAccountBalanceAmountRollback(SubtractAccountBalanceAmountRollbackParameterDTO subtractAccountBalanceAmountRollbackParameterDTO, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    if (subtractAccountBalanceAmountRollbackParameterDTO.Amount < 0)
                    {
                        throw new Exception("Amount to add must be greater than or equal to 0");
                    }

                    var account = await _accountGenericRepository.FindByIdAsync(subtractAccountBalanceAmountRollbackParameterDTO.AccountId);
                    if (account == null)
                    {
                        throw new Exception("Account with id " + subtractAccountBalanceAmountRollbackParameterDTO.AccountId + " does not exist");
                    }
                    else if (account.IsVerified == false)
                    {
                        throw new Exception("Account with id " + subtractAccountBalanceAmountRollbackParameterDTO.AccountId + " is not verified");
                    }


                    account.Balance += subtractAccountBalanceAmountRollbackParameterDTO.Amount;
                    await _accountGenericRepository.UpdateAsync(account.Id, account);

                    await transaction.CommitAsync();

                    var messageNextRequestData = command.RequestData;
                    messageNextRequestData["AccountId"] = subtractAccountBalanceAmountRollbackParameterDTO.AccountId;
                    messageNextRequestData["Amount"] = subtractAccountBalanceAmountRollbackParameterDTO.Amount;

                    var messageResponseData = JObject.FromObject(new
                    {
                        // Message = "Add account balance amount successfully",
                        AccountId = subtractAccountBalanceAmountRollbackParameterDTO.AccountId,
                        Amount = subtractAccountBalanceAmountRollbackParameterDTO.Amount,
                        NewBalance = account.Balance,
                    });
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.UserManagementDomain,
                        requestData: messageNextRequestData,
                        responseData: messageResponseData,
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "subtract-account-balance-amount-rollback.success"
                        );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);

                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();

                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.UserManagementDomain,
                        requestData: command.RequestData,
                        responseData: JObject.FromObject(new
                        {
                            ErrorMessage = $"Subtract account balance amount rollback failed, error: {ex.Message}"
                        }),
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "subtract-account-balance-amount-rollback.failed"
                        );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);
                    Console.WriteLine("\n" + ex.StackTrace + "\n");
                }
            }
        }

        public async Task AddPodcasterBalanceAmount(AddPodcasterBalanceAmountParameterDTO addPodcasterBalanceAmountParameterDTO, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    if (addPodcasterBalanceAmountParameterDTO.Amount < 0)
                    {
                        throw new Exception("Amount to add must be greater than or equal to 0");
                    }

                    var account = await _accountGenericRepository.FindByIdAsync(addPodcasterBalanceAmountParameterDTO.PodcasterId,
                    includeFunc: query => query.Include(a => a.PodcasterProfile));
                    if (account == null)
                    {
                        throw new Exception("Account with id " + addPodcasterBalanceAmountParameterDTO.PodcasterId + " does not exist");
                    }
                    else if (account.IsVerified == false)
                    {
                        throw new Exception("Account with id " + addPodcasterBalanceAmountParameterDTO.PodcasterId + " is not verified");
                    }
                    else if (account.PodcasterProfile == null)
                    {
                        throw new Exception("Account with id " + addPodcasterBalanceAmountParameterDTO.PodcasterId + " is not a podcaster");
                    }
                    else if (account.PodcasterProfile.IsVerified == false)
                    {
                        throw new Exception("Podcaster profile of account with id " + addPodcasterBalanceAmountParameterDTO.PodcasterId + " is not verified");
                    }

                    account.Balance += addPodcasterBalanceAmountParameterDTO.Amount;
                    await _accountGenericRepository.UpdateAsync(account.Id, account);
                    await transaction.CommitAsync();
                    var messageNextRequestData = command.RequestData;
                    messageNextRequestData["PodcasterId"] = addPodcasterBalanceAmountParameterDTO.PodcasterId;
                    messageNextRequestData["Amount"] = addPodcasterBalanceAmountParameterDTO.Amount;
                    var messageResponseData = JObject.FromObject(new
                    {
                        // Message = "Add podcaster balance amount successfully",
                        PodcasterId = addPodcasterBalanceAmountParameterDTO.PodcasterId,
                        Amount = addPodcasterBalanceAmountParameterDTO.Amount,
                        NewBalance = account.Balance,
                    });
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.UserManagementDomain,
                        requestData: messageNextRequestData,
                        responseData: messageResponseData,
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "add-podcaster-balance-amount.success"
                    );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.UserManagementDomain,
                        requestData: command.RequestData,
                        responseData: JObject.FromObject(new
                        {
                            ErrorMessage = $"Add podcaster balance amount failed, error: {ex.Message}"
                        }),
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "add-podcaster-balance-amount.failed"
                    );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);
                    Console.WriteLine("\n" + ex.StackTrace + "\n");
                }
            }
        }


        public async Task AddPodcasterBalanceAmountRollback(AddPodcasterBalanceAmountRollbackParameterDTO addPodcasterBalanceAmountRollbackParameterDTO, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    if (addPodcasterBalanceAmountRollbackParameterDTO.Amount < 0)
                    {
                        throw new Exception("Amount to subtract must be greater than or equal to 0");
                    }

                    var account = await _accountGenericRepository.FindByIdAsync(addPodcasterBalanceAmountRollbackParameterDTO.PodcasterId,
                    includeFunc: query => query.Include(a => a.PodcasterProfile));
                    if (account == null)
                    {
                        throw new Exception("Account with id " + addPodcasterBalanceAmountRollbackParameterDTO.PodcasterId + " does not exist");
                    }
                    else if (account.IsVerified == false)
                    {
                        throw new Exception("Account with id " + addPodcasterBalanceAmountRollbackParameterDTO.PodcasterId + " is not verified");
                    }
                    else if (account.PodcasterProfile == null)
                    {
                        throw new Exception("Account with id " + addPodcasterBalanceAmountRollbackParameterDTO.PodcasterId + " is not a podcaster");
                    }
                    else if (account.PodcasterProfile.IsVerified == false)
                    {
                        throw new Exception("Podcaster profile of account with id " + addPodcasterBalanceAmountRollbackParameterDTO.PodcasterId + " is not verified");
                    }

                    var isNegativeBalance = false;
                    account.Balance -= addPodcasterBalanceAmountRollbackParameterDTO.Amount;
                    if (account.Balance < 0)
                    {
                        account.Balance = 0;
                        isNegativeBalance = true;
                    }


                    await _accountGenericRepository.UpdateAsync(account.Id, account);

                    if (isNegativeBalance == true)
                    {
                        throw new Exception("Podcaster balance cannot be negative, account balance is set to 0 instead of subsctract result of " + addPodcasterBalanceAmountRollbackParameterDTO.Amount);
                    }

                    await transaction.CommitAsync();

                    var messageNextRequestData = command.RequestData;
                    messageNextRequestData["PodcasterId"] = addPodcasterBalanceAmountRollbackParameterDTO.PodcasterId;
                    messageNextRequestData["Amount"] = addPodcasterBalanceAmountRollbackParameterDTO.Amount;

                    var messageResponseData = JObject.FromObject(new
                    {
                        // Message = "Subtract podcaster balance amount successfully",
                        PodcasterId = addPodcasterBalanceAmountRollbackParameterDTO.PodcasterId,
                        Amount = addPodcasterBalanceAmountRollbackParameterDTO.Amount,
                        NewBalance = account.Balance,
                    });
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.UserManagementDomain,
                        requestData: messageNextRequestData,
                        responseData: messageResponseData,
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "add-podcaster-balance-amount-rollback.success"
                        );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);

                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();

                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.UserManagementDomain,
                        requestData: command.RequestData,
                        responseData: JObject.FromObject(new
                        {
                            ErrorMessage = $"Add podcaster balance amount rollback failed, error: {ex.Message}"
                        }),
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "add-podcaster-balance-amount-rollback.failed"
                        );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);
                    Console.WriteLine("\n" + ex.StackTrace + "\n");
                }
            }
        }

        public async Task SubtractAccountListenSlot(SubtractAccountListenSlotParameterDTO subtractAccountListenSlotParameterDTO, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    if (subtractAccountListenSlotParameterDTO.PodcastListenSlotAmount < 0)
                    {
                        throw new Exception("Podcast listen slot amount to subtract must be greater than or equal to 0");
                    }

                    var account = await _accountGenericRepository.FindByIdAsync(subtractAccountListenSlotParameterDTO.AccountId);
                    if (account == null)
                    {
                        throw new Exception("Account with id " + subtractAccountListenSlotParameterDTO.AccountId + " does not exist");
                    }
                    else if (account.IsVerified == false)
                    {
                        throw new Exception("Account with id " + subtractAccountListenSlotParameterDTO.AccountId + " is not verified");
                    }

                    account.PodcastListenSlot -= subtractAccountListenSlotParameterDTO.PodcastListenSlotAmount;
                    account.LastPodcastListenSlotChanged = _dateHelper.GetNowByAppTimeZone();
                    if (account.PodcastListenSlot < 0)
                    {
                        account.PodcastListenSlot = 0;
                    }

                    await _accountGenericRepository.UpdateAsync(account.Id, account);

                    await transaction.CommitAsync();

                    var messageNextRequestData = command.RequestData;
                    messageNextRequestData["AccountId"] = subtractAccountListenSlotParameterDTO.AccountId;
                    messageNextRequestData["PodcastListenSlotAmount"] = subtractAccountListenSlotParameterDTO.PodcastListenSlotAmount;

                    var messageResponseData = JObject.FromObject(new
                    {
                        // Message = "Subtract account podcast listen slot successfully",
                        AccountId = subtractAccountListenSlotParameterDTO.AccountId,
                        PodcastListenSlotAmount = subtractAccountListenSlotParameterDTO.PodcastListenSlotAmount,
                        NewPodcastListenSlot = account.PodcastListenSlot,
                    });
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.UserManagementDomain,
                        requestData: messageNextRequestData,
                        responseData: messageResponseData,
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "subtract-account-podcast-listen-slot.success"
                        );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);

                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();

                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.UserManagementDomain,
                        requestData: command.RequestData,
                        responseData: JObject.FromObject(new
                        {
                            ErrorMessage = $"Subtract account podcast listen slot failed, error: {ex.Message}"
                        }),
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "subtract-account-podcast-listen-slot.failed"
                        );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);
                    Console.WriteLine("\n" + ex.StackTrace + "\n");
                }
            }
        }

        public async Task AddPodcasterListenCount(AddPodcasterListenCountParameterDTO addPodcasterListenCountParameterDTO, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    if (addPodcasterListenCountParameterDTO.ListenCountAmount < 0)
                    {
                        throw new Exception("Listen count amount to add must be greater than or equal to 0");
                    }

                    var account = await _accountGenericRepository.FindByIdAsync(addPodcasterListenCountParameterDTO.PodcasterId,
                    includeFunc: query => query.Include(a => a.PodcasterProfile));
                    if (account == null)
                    {
                        throw new Exception("Account with id " + addPodcasterListenCountParameterDTO.PodcasterId + " does not exist");
                    }
                    else if (account.IsVerified == false)
                    {
                        throw new Exception("Account with id " + addPodcasterListenCountParameterDTO.PodcasterId + " is not verified");
                    }
                    else if (account.PodcasterProfile == null)
                    {
                        throw new Exception("Account with id " + addPodcasterListenCountParameterDTO.PodcasterId + " is not a podcaster");
                    }
                    else if (account.PodcasterProfile.IsVerified == false)
                    {
                        throw new Exception("Podcaster profile of account with id " + addPodcasterListenCountParameterDTO.PodcasterId + " is not verified");
                    }

                    account.PodcasterProfile.ListenCount += addPodcasterListenCountParameterDTO.ListenCountAmount;
                    // await _accountGenericRepository.UpdateAsync(account.Id, account);
                    await _podcasterProfileGenericRepository.UpdateAsync(account.PodcasterProfile.AccountId, account.PodcasterProfile);
                    await transaction.CommitAsync();
                    var messageNextRequestData = command.RequestData;
                    messageNextRequestData["PodcasterId"] = addPodcasterListenCountParameterDTO.PodcasterId;
                    messageNextRequestData["ListenCountAmount"] = addPodcasterListenCountParameterDTO.ListenCountAmount;
                    var messageResponseData = JObject.FromObject(new
                    {
                        // Message = "Add podcaster listen count successfully",
                        PodcasterId = addPodcasterListenCountParameterDTO.PodcasterId,
                        ListenCountAmount = addPodcasterListenCountParameterDTO.ListenCountAmount,
                        NewListenCount = account.PodcasterProfile.ListenCount,
                    });
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.UserManagementDomain,
                        requestData: messageNextRequestData,
                        responseData: messageResponseData,
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "add-podcaster-listen-count.success"
                    );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.UserManagementDomain,
                        requestData: command.RequestData,
                        responseData: JObject.FromObject(new
                        {
                            ErrorMessage = $"Add podcaster listen count failed, error: {ex.Message}"
                        }),
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "add-podcaster-listen-count.failed"
                    );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);
                    Console.WriteLine("\n" + ex.StackTrace + "\n");
                }
            }
        }

        public async Task DeleteAccountSavedEpisodeDMCARemoveEpisodeForce(DeleteAccountSavedEpisodeDMCARemoveEpisodeForceParameterDTO deleteAccountSavedEpisodeDMCARemoveEpisodeForceParameterDTO, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {

                    List<int> affectedAccountIds = await _unitOfWork.AccountSavedPodcastEpisodeRepository.DeleteByPodcastEpisodeIdAsync(deleteAccountSavedEpisodeDMCARemoveEpisodeForceParameterDTO.PodcastEpisodeId);

                    await transaction.CommitAsync();

                    var messageNextRequestData = command.RequestData;
                    messageNextRequestData["PodcastEpisodeId"] = deleteAccountSavedEpisodeDMCARemoveEpisodeForceParameterDTO.PodcastEpisodeId;
                    var messageResponseData = JObject.FromObject(new
                    {
                        // Message = "Delete episode saved successfully",
                        PodcastEpisodeId = deleteAccountSavedEpisodeDMCARemoveEpisodeForceParameterDTO.PodcastEpisodeId,
                    });
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.UserManagementDomain,
                        requestData: messageNextRequestData,
                        responseData: messageResponseData,
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "delete-account-saved-episode-dmca-remove-episode-force.success"
                    );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();

                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.UserManagementDomain,
                        requestData: command.RequestData,
                        responseData: JObject.FromObject(new
                        {
                            ErrorMessage = $"Delete account saved episode DMCA remove episode force failed, error: {ex.Message}"
                        }),
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "delete-account-saved-episode-dmca-remove-episode-force.failed"
                    );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);
                    Console.WriteLine("\n" + ex.StackTrace + "\n");
                }
            }
        }

        public async Task DeleteAccountSavedEpisodeUnpublishEpisodeForce(DeleteAccountSavedEpisodeUnpublishEpisodeForceParameterDTO deleteAccountSavedEpisodeUnpublishEpisodeForceParameterDTO, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {

                    List<int> affectedAccountIds = await _unitOfWork.AccountSavedPodcastEpisodeRepository.DeleteByPodcastEpisodeIdAsync(deleteAccountSavedEpisodeUnpublishEpisodeForceParameterDTO.PodcastEpisodeId);

                    await transaction.CommitAsync();

                    var messageNextRequestData = command.RequestData;
                    messageNextRequestData["PodcastEpisodeId"] = deleteAccountSavedEpisodeUnpublishEpisodeForceParameterDTO.PodcastEpisodeId;
                    var messageResponseData = JObject.FromObject(new
                    {
                        // Message = "Delete episode saved successfully",
                        PodcastEpisodeId = deleteAccountSavedEpisodeUnpublishEpisodeForceParameterDTO.PodcastEpisodeId,
                    });
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.UserManagementDomain,
                        requestData: messageNextRequestData,
                        responseData: messageResponseData,
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "delete-account-saved-episode-unpublish-episode-force.success"
                    );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();

                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.UserManagementDomain,
                        requestData: command.RequestData,
                        responseData: JObject.FromObject(new
                        {
                            ErrorMessage = $"Delete account saved episode unpublish episode force failed, error: {ex.Message}"
                        }),
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "delete-account-saved-episode-unpublish-episode-force.failed"
                    );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);
                    Console.WriteLine("\n" + ex.StackTrace + "\n");
                }
            }
        }

        public async Task DeleteAccountFollowedShowDMCARemoveShowForce(DeleteAccountFollowedShowDMCARemoveShowForceParameterDTO deleteAccountFollowedShowDMCARemoveShowForceParameterDTO, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {

                    List<int> affectedAccountIds = await _unitOfWork.AccountFollowedPodcastShowRepository.DeleteByPodcastShowIdAsync(deleteAccountFollowedShowDMCARemoveShowForceParameterDTO.PodcastShowId);

                    await transaction.CommitAsync();

                    var messageNextRequestData = command.RequestData;
                    messageNextRequestData["PodcastShowId"] = deleteAccountFollowedShowDMCARemoveShowForceParameterDTO.PodcastShowId;
                    var messageResponseData = JObject.FromObject(new
                    {
                        // Message = "Delete followed show successfully",
                        PodcastShowId = deleteAccountFollowedShowDMCARemoveShowForceParameterDTO.PodcastShowId,
                    });
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.UserManagementDomain,
                        requestData: messageNextRequestData,
                        responseData: messageResponseData,
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "delete-account-followed-show-dmca-remove-show-force.success"
                    );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();

                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.UserManagementDomain,
                        requestData: command.RequestData,
                        responseData: JObject.FromObject(new
                        {
                            ErrorMessage = $"Delete account followed show DMCA remove show force failed, error: {ex.Message}"
                        }),
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "delete-account-followed-show-dmca-remove-show-force.failed"
                    );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);
                    Console.WriteLine("\n" + ex.StackTrace + "\n");
                }
            }
        }

        public async Task DeleteAccountFollowedShowUnpublishShowForce(DeleteAccountFollowedShowUnpublishShowForceParameterDTO deleteAccountFollowedShowUnpublishShowForceParameterDTO, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {

                    List<int> affectedAccountIds = await _unitOfWork.AccountFollowedPodcastShowRepository.DeleteByPodcastShowIdAsync(deleteAccountFollowedShowUnpublishShowForceParameterDTO.PodcastShowId);

                    await transaction.CommitAsync();

                    var messageNextRequestData = command.RequestData;
                    messageNextRequestData["PodcastShowId"] = deleteAccountFollowedShowUnpublishShowForceParameterDTO.PodcastShowId;
                    var messageResponseData = JObject.FromObject(new
                    {
                        // Message = "Delete followed show successfully",
                        PodcastShowId = deleteAccountFollowedShowUnpublishShowForceParameterDTO.PodcastShowId,
                    });
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.UserManagementDomain,
                        requestData: messageNextRequestData,
                        responseData: messageResponseData,
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "delete-account-followed-show-unpublish-show-force.success"
                    );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();

                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.UserManagementDomain,
                        requestData: command.RequestData,
                        responseData: JObject.FromObject(new
                        {
                            ErrorMessage = $"Delete account followed show unpublish show force failed, error: {ex.Message}"
                        }),
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "delete-account-followed-show-unpublish-show-force.failed"
                    );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);
                    Console.WriteLine("\n" + ex.StackTrace + "\n");
                }
            }
        }

        public async Task DeleteAccountSavedShowEpisodesDMCARemoveShowForce(DeleteAccountSavedShowEpisodesDMCARemoveShowForceParameterDTO deleteAccountSavedShowEpisodesDMCARemoveShowForceParameterDTO, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {

                    var batchRequest = new BatchQueryRequest
                    {
                        Queries = new List<BatchQueryItem>
                        {
                            new BatchQueryItem
                            {
                                Key = "podcastEpisode",
                                QueryType = "findall",
                                EntityType = "PodcastEpisode",
                                Parameters = JObject.FromObject(new
                                {
                                    where = new
                                    {
                                        PodcastShowId = deleteAccountSavedShowEpisodesDMCARemoveShowForceParameterDTO.PodcastShowId
                                    },
                                }),
                            }
                        }
                    };

                    var result = await _httpServiceQueryClient.ExecuteBatchAsync("PodcastService", batchRequest);


                    var podcastEpisode = result.Results["podcastEpisode"].ToObject<List<PodcastEpisodeDTO>>();

                    List<Guid> podcastEpisodeIds = podcastEpisode.Select(pe => pe.Id).ToList();

                    foreach (var podcastEpisodeId in podcastEpisodeIds)
                    {
                        List<int> affectedAccountIds = await _unitOfWork.AccountSavedPodcastEpisodeRepository.DeleteByPodcastEpisodeIdAsync(podcastEpisodeId);
                    }

                    await transaction.CommitAsync();

                    var messageNextRequestData = command.RequestData;
                    messageNextRequestData["PodcastShowId"] = deleteAccountSavedShowEpisodesDMCARemoveShowForceParameterDTO.PodcastShowId;
                    var messageResponseData = JObject.FromObject(new
                    {
                        // Message = "Delete saved show successfully",
                        PodcastShowId = deleteAccountSavedShowEpisodesDMCARemoveShowForceParameterDTO.PodcastShowId,
                    });
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.UserManagementDomain,
                        requestData: messageNextRequestData,
                        responseData: messageResponseData,
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "delete-account-saved-show-episodes-dmca-remove-show-force.success"
                    );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();

                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.UserManagementDomain,
                        requestData: command.RequestData,
                        responseData: JObject.FromObject(new
                        {
                            ErrorMessage = $"Delete account saved show episodes DMCA remove show force failed, error: {ex.Message}"
                        }),
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "delete-account-saved-show-episodes-dmca-remove-show-force.failed"
                    );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);
                    Console.WriteLine("\n" + ex.StackTrace + "\n");
                }
            }
        }

        public async Task DeleteAccountSavedShowEpisodesUnpublishShowForce(DeleteAccountSavedShowEpisodesUnpublishShowForceParameterDTO deleteAccountSavedShowEpisodesUnpublishShowForceParameterDTO, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    foreach (var podcastEpisodeId in deleteAccountSavedShowEpisodesUnpublishShowForceParameterDTO.DmcaDismissedEpisodeIds)
                    {
                        List<int> affectedAccountIds = await _unitOfWork.AccountSavedPodcastEpisodeRepository.DeleteByPodcastEpisodeIdAsync(podcastEpisodeId);
                    }

                    await transaction.CommitAsync();
                    var messageNextRequestData = command.RequestData;
                    messageNextRequestData["PodcastShowId"] = deleteAccountSavedShowEpisodesUnpublishShowForceParameterDTO.PodcastShowId;
                    messageNextRequestData["DmcaDismissedEpisodeIds"] = JArray.FromObject(deleteAccountSavedShowEpisodesUnpublishShowForceParameterDTO.DmcaDismissedEpisodeIds);
                    var messageResponseData = JObject.FromObject(new
                    {
                        // Message = "Delete saved show successfully",
                        PodcastShowId = deleteAccountSavedShowEpisodesUnpublishShowForceParameterDTO.PodcastShowId,
                        DmcaDismissedEpisodeIds = deleteAccountSavedShowEpisodesUnpublishShowForceParameterDTO.DmcaDismissedEpisodeIds,
                    });
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.UserManagementDomain,
                        requestData: messageNextRequestData,
                        responseData: messageResponseData,
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "delete-account-saved-show-episodes-unpublish-show-force.success"
                    );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();

                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.UserManagementDomain,
                        requestData: command.RequestData,
                        responseData: JObject.FromObject(new
                        {
                            ErrorMessage = $"Delete account saved show episodes unpublish show force failed, error: {ex.Message}"
                        }),
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "delete-account-saved-show-episodes-unpublish-show-force.failed"
                    );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);
                    Console.WriteLine("\n" + ex.StackTrace + "\n");
                }
            }

        }

        public async Task DeleteAccountSavedShowEpisodesShowDeletionForce(DeleteAccountSavedShowEpisodesShowDeletionForceParameterDTO deleteAccountSavedShowEpisodesShowDeletionForceParameterDTO, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    // lấy ra danh sách episode của show để xoá 
                    var batchRequest = new BatchQueryRequest
                    {
                        Queries = new List<BatchQueryItem>
                        {
                            new BatchQueryItem
                            {
                                Key = "podcastEpisode",
                                QueryType = "findall",
                                EntityType = "PodcastEpisode",
                                Parameters = JObject.FromObject(new
                                {
                                    where = new
                                    {
                                        PodcastShowId = deleteAccountSavedShowEpisodesShowDeletionForceParameterDTO.PodcastShowId
                                    },
                                }),
                            }
                        }
                    };

                    var result = await _httpServiceQueryClient.ExecuteBatchAsync("PodcastService", batchRequest);


                    var podcastEpisode = result.Results["podcastEpisode"].ToObject<List<PodcastEpisodeDTO>>();

                    List<Guid> podcastEpisodeIds = podcastEpisode.Select(pe => pe.Id).ToList();

                    foreach (var podcastEpisodeId in podcastEpisodeIds)
                    {
                        List<int> affectedAccountIds = await _unitOfWork.AccountSavedPodcastEpisodeRepository.DeleteByPodcastEpisodeIdAsync(podcastEpisodeId);
                    }


                    await transaction.CommitAsync();

                    var messageNextRequestData = command.RequestData;
                    messageNextRequestData["PodcastShowId"] = deleteAccountSavedShowEpisodesShowDeletionForceParameterDTO.PodcastShowId;
                    var messageResponseData = JObject.FromObject(new
                    {
                        // Message = "Delete saved show successfully",
                        PodcastShowId = deleteAccountSavedShowEpisodesShowDeletionForceParameterDTO.PodcastShowId,
                    });
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.UserManagementDomain,
                        requestData: messageNextRequestData,
                        responseData: messageResponseData,
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "delete-account-saved-show-episodes-show-deletion-force.success"
                    );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();

                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.UserManagementDomain,
                        requestData: command.RequestData,
                        responseData: JObject.FromObject(new
                        {
                            ErrorMessage = $"Delete account saved show episodes show deletion force failed, error: {ex.Message}"
                        }),
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "delete-account-saved-show-episodes-show-deletion-force.failed"
                    );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);
                    Console.WriteLine("\n" + ex.StackTrace + "\n");
                }
            }
        }

        public async Task DeleteAccountSavedChannelEpisodesUnpublishChannelForce(DeleteAccountSavedChannelEpisodesUnpublishChannelForceParameterDTO deleteAccountSavedChannelEpisodesUnpublishChannelForceParameterDTO, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {

                    foreach (var podcastEpisodeId in deleteAccountSavedChannelEpisodesUnpublishChannelForceParameterDTO.DmcaDismissedEpisodeIds)
                    {
                        List<int> affectedAccountIds = await _unitOfWork.AccountSavedPodcastEpisodeRepository.DeleteByPodcastEpisodeIdAsync(podcastEpisodeId);
                    }
                    await transaction.CommitAsync();

                    var messageNextRequestData = command.RequestData;
                    messageNextRequestData["PodcastChannelId"] = deleteAccountSavedChannelEpisodesUnpublishChannelForceParameterDTO.PodcastChannelId;
                    messageNextRequestData["DmcaDismissedEpisodeIds"] = JArray.FromObject(deleteAccountSavedChannelEpisodesUnpublishChannelForceParameterDTO.DmcaDismissedEpisodeIds);
                    var messageResponseData = JObject.FromObject(new
                    {
                        // Message = "Delete saved channel episodes successfully",
                        PodcastChannelId = deleteAccountSavedChannelEpisodesUnpublishChannelForceParameterDTO.PodcastChannelId,
                        DmcaDismissedEpisodeIds = deleteAccountSavedChannelEpisodesUnpublishChannelForceParameterDTO.DmcaDismissedEpisodeIds,
                    });
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.UserManagementDomain,
                        requestData: messageNextRequestData,
                        responseData: messageResponseData,
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "delete-account-saved-channel-episodes-unpublish-channel-force.success"
                    );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();

                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.UserManagementDomain,
                        requestData: command.RequestData,
                        responseData: JObject.FromObject(new
                        {
                            ErrorMessage = $"Delete account saved channel episode unpublish channel force failed, error: {ex.Message}"
                        }),
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "delete-account-saved-channel-episodes-unpublish-channel-force.failed"
                    );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);
                    Console.WriteLine("\n" + ex.StackTrace + "\n");
                }
            }
        }

        public async Task DeleteAccountSavedChannelEpisodesChannelDeletionForce(DeleteAccountSavedChannelEpisodesChannelDeletionForceParameterDTO deleteAccountSavedChannelEpisodesChannelDeletionForceParameterDTO, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {

                    var batchRequest = new BatchQueryRequest
                    {
                        Queries = new List<BatchQueryItem>
                        {
                            new BatchQueryItem
                            {
                                Key = "podcastChannel",
                                QueryType = "findbyid",
                                EntityType = "PodcastChannel",
                                Parameters = JObject.FromObject(new
                                {
                                    where =  new {
                                    DeletedAt = (DateTime?)null,
                                    },
                                    id = "7c50ec4d-dd32-47c8-955a-084c2cc640bf",
                                    include = "PodcastShows, PodcastShows.PodcastEpisodes"
                                }),
                                // Fields = new string[] { "PodcastShows.PodcastEpisodes.Id" }
                                Fields = new[] {
                                    "PodcastShows",
                                }
                            }
                        }
                    };

                    var result = await _httpServiceQueryClient.ExecuteBatchAsync("PodcastService", batchRequest);

                    // var podcastEpisodeIds = result.Results["podcastChannel"]["PodcastShows"]
                    //     .SelectMany(ps => ps["PodcastEpisodes"])
                    //     .Select(pe => pe["Id"].ToObject<Guid>())
                    //     .ToList();
                    var podcastChannel = result.Results["podcastChannel"].ToObject<PodcastChannelDTO>();
                    var podcastEpisodeIds = podcastChannel.PodcastShows.SelectMany(ps => ps.PodcastEpisodes).Select(pe => pe.Id).ToList();
                    foreach (var podcastEpisodeId in podcastEpisodeIds)
                    {
                        List<int> affectedAccountIds = await _unitOfWork.AccountSavedPodcastEpisodeRepository.DeleteByPodcastEpisodeIdAsync(podcastEpisodeId);
                    }
                    await transaction.CommitAsync();
                    var messageNextRequestData = command.RequestData;
                    messageNextRequestData["PodcastChannelId"] = deleteAccountSavedChannelEpisodesChannelDeletionForceParameterDTO.PodcastChannelId;
                    var messageResponseData = JObject.FromObject(new
                    {
                        // Message = "Delete saved channel episodes successfully",
                        PodcastChannelId = deleteAccountSavedChannelEpisodesChannelDeletionForceParameterDTO.PodcastChannelId,
                    });
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.UserManagementDomain,
                        requestData: messageNextRequestData,
                        responseData: messageResponseData,
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "delete-account-saved-channel-episodes-channel-deletion-force.success"
                    );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();

                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.UserManagementDomain,
                        requestData: command.RequestData,
                        responseData: JObject.FromObject(new
                        {
                            ErrorMessage = $"Delete account saved channel episodes channel deletion force failed, error: {ex.Message}"
                        }),
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "delete-account-saved-channel-episodes-channel-deletion-force.failed"
                    );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);
                    Console.WriteLine("\n" + ex.StackTrace + "\n");
                }
            }
        }

        public async Task DeleteAccountSavedPodcasterEpisodesTerminatePodcasterForce(DeleteAccountSavedPodcasterEpisodesTerminatePodcasterForceParameterDTO deleteAccountSavedPodcasterEpisodesTerminatePodcasterForceParameterDTO, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {

                    var batchRequest = new BatchQueryRequest
                    {
                        Queries = new List<BatchQueryItem>
                        {
                            new BatchQueryItem
                            {
                                Key = "podcastShow",
                                QueryType = "findall",
                                EntityType = "PodcastShow",
                                Parameters = JObject.FromObject(new
                                {
                                    where = new
                                    {
                                        PodcasterId = 17
                                    },
                                    include = "PodcastEpisodes"
                                }),
                            }
                        }
                    };

                    var result = await _httpServiceQueryClient.ExecuteBatchAsync("PodcastService", batchRequest);


                    var podcastShow = result.Results["podcastShow"].ToObject<List<PodcastShowDTO>>();

                    List<Guid> podcastEpisodeIds = podcastShow.SelectMany(ps => ps.PodcastEpisodes).Select(pe => pe.Id).ToList();

                    foreach (var podcastEpisodeId in podcastEpisodeIds)
                    {
                        List<int> affectedAccountIds = await _unitOfWork.AccountSavedPodcastEpisodeRepository.DeleteByPodcastEpisodeIdAsync(podcastEpisodeId);
                    }

                    await transaction.CommitAsync();

                    var messageNextRequestData = command.RequestData;
                    messageNextRequestData["PodcasterId"] = deleteAccountSavedPodcasterEpisodesTerminatePodcasterForceParameterDTO.PodcasterId;
                    var messageResponseData = JObject.FromObject(new
                    {
                        // Message = "Delete saved podcaster episodes successfully",
                        PodcasterId = deleteAccountSavedPodcasterEpisodesTerminatePodcasterForceParameterDTO.PodcasterId,
                    });
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.UserManagementDomain,
                        requestData: messageNextRequestData,
                        responseData: messageResponseData,
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "delete-account-saved-podcaster-episodes-terminate-podcaster-force.success"
                    );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();

                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.UserManagementDomain,
                        requestData: command.RequestData,
                        responseData: JObject.FromObject(new
                        {
                            ErrorMessage = $"Delete account saved podcaster episodes terminate podcaster force failed, error: {ex.Message}"
                        }),
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "delete-account-saved-podcaster-episodes-terminate-podcaster-force.failed"
                    );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);
                    Console.WriteLine("\n" + ex.StackTrace + "\n");
                }
            }
        }

        public async Task DeleteAccountSavedEpisodeEpisodeDeletionForce(DeleteAccountSavedEpisodeEpisodeDeletionForceParameterDTO deleteAccountSavedEpisodeEpisodeDeletionForceParameterDTO, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {

                    List<int> affectedAccountIds = await _unitOfWork.AccountSavedPodcastEpisodeRepository.DeleteByPodcastEpisodeIdAsync(deleteAccountSavedEpisodeEpisodeDeletionForceParameterDTO.PodcastEpisodeId);

                    await transaction.CommitAsync();

                    var messageNextRequestData = command.RequestData;
                    messageNextRequestData["PodcastEpisodeId"] = deleteAccountSavedEpisodeEpisodeDeletionForceParameterDTO.PodcastEpisodeId;
                    var messageResponseData = JObject.FromObject(new
                    {
                        // Message = "Delete episode saved successfully",
                        PodcastEpisodeId = deleteAccountSavedEpisodeEpisodeDeletionForceParameterDTO.PodcastEpisodeId,
                    });
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.UserManagementDomain,
                        requestData: messageNextRequestData,
                        responseData: messageResponseData,
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "delete-account-saved-episode-episode-deletion-force.success"
                    );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();

                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.UserManagementDomain,
                        requestData: command.RequestData,
                        responseData: JObject.FromObject(new
                        {
                            ErrorMessage = $"Delete account saved episode episode deletion force failed, error: {ex.Message}"
                        }),
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "delete-account-saved-episode-episode-deletion-force.failed"
                    );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);
                    Console.WriteLine("\n" + ex.StackTrace + "\n");
                }
            }
        }

        public async Task DeleteAccountFavoritedChannelUnpublishChannelForce(DeleteAccountFavoritedChannelUnpublishChannelForceParameterDTO deleteAccountFavoritedChannelUnpublishChannelForceParameterDTO, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {

                    List<int> affectedAccountIds = await _unitOfWork.AccountFavoritedPodcastChannelRepository.DeleteByPodcastChannelIdAsync(deleteAccountFavoritedChannelUnpublishChannelForceParameterDTO.PodcastChannelId);

                    await transaction.CommitAsync();

                    var messageNextRequestData = command.RequestData;
                    messageNextRequestData["PodcastChannelId"] = deleteAccountFavoritedChannelUnpublishChannelForceParameterDTO.PodcastChannelId;
                    var messageResponseData = JObject.FromObject(new
                    {
                        // Message = "Delete favorited channel successfully",
                        PodcastChannelId = deleteAccountFavoritedChannelUnpublishChannelForceParameterDTO.PodcastChannelId,
                    });
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.UserManagementDomain,
                        requestData: messageNextRequestData,
                        responseData: messageResponseData,
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "delete-account-favorited-channel-unpublish-channel-force.success"
                    );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();

                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.UserManagementDomain,
                        requestData: command.RequestData,
                        responseData: JObject.FromObject(new
                        {
                            ErrorMessage = $"Delete account favorited channel unpublish channel force failed, error: {ex.Message}"
                        }),
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "delete-account-favorited-channel-unpublish-channel-force.failed"
                    );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);
                    Console.WriteLine("\n" + ex.StackTrace + "\n");
                }
            }
        }

        public async Task DeleteAccountFavoritedChannelChannelDeletionForce(DeleteAccountFavoritedChannelChannelDeletionForceParameterDTO deleteAccountFavoritedChannelChannelDeletionForceParameterDTO, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {

                    List<int> affectedAccountIds = await _unitOfWork.AccountFavoritedPodcastChannelRepository.DeleteByPodcastChannelIdAsync(deleteAccountFavoritedChannelChannelDeletionForceParameterDTO.PodcastChannelId);

                    await transaction.CommitAsync();

                    var messageNextRequestData = command.RequestData;
                    messageNextRequestData["PodcastChannelId"] = deleteAccountFavoritedChannelChannelDeletionForceParameterDTO.PodcastChannelId;
                    var messageResponseData = JObject.FromObject(new
                    {
                        // Message = "Delete favorited channel successfully",
                        PodcastChannelId = deleteAccountFavoritedChannelChannelDeletionForceParameterDTO.PodcastChannelId,
                    });
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.UserManagementDomain,
                        requestData: messageNextRequestData,
                        responseData: messageResponseData,
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "delete-account-favorited-channel-channel-deletion-force.success"
                    );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();

                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.UserManagementDomain,
                        requestData: command.RequestData,
                        responseData: JObject.FromObject(new
                        {
                            ErrorMessage = $"Delete account favorited channel channel deletion force failed, error: {ex.Message}"
                        }),
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "delete-account-favorited-channel-channel-deletion-force.failed"
                    );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);
                    Console.WriteLine("\n" + ex.StackTrace + "\n");
                }
            }
        }

        public async Task DeleteAccountFavoritedPodcasterChannelsTerminatePodcasterForce(DeleteAccountFavoritedPodcasterChannelsTerminatePodcasterForceParameterDTO deleteAccountFavoritedPodcasterChannelsTerminatePodcasterForceParameterDTO, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var batchRequest = new BatchQueryRequest
                    {
                        Queries = new List<BatchQueryItem>
                        {
                            new BatchQueryItem
                            {
                                Key = "podcastChannels",
                                QueryType = "findall",
                                EntityType = "PodcastChannel",
                                Parameters = JObject.FromObject(new
                                {
                                    where =  new {
                                        PodcasterId = deleteAccountFavoritedPodcasterChannelsTerminatePodcasterForceParameterDTO.PodcasterId,
                                        DeletedAt = (DateTime?)null,
                                    },
                                }),
                                Fields = new[] {
                                    "Id",
                                }
                            }
                        }
                    };

                    var result = await _httpServiceQueryClient.ExecuteBatchAsync("PodcastService", batchRequest);
                    var podcastChannels = result.Results["podcastChannels"].ToObject<List<PodcastChannelDTO>>();
                    var podcastEpisodeIds = podcastChannels.Select(pc => pc.Id).ToList();

                    foreach (var podcastChannelId in podcastEpisodeIds)
                    {
                        List<int> affectedAccountIds = await _unitOfWork.AccountFavoritedPodcastChannelRepository.DeleteByPodcastChannelIdAsync(podcastChannelId);
                    }

                    await transaction.CommitAsync();

                    var messageNextRequestData = command.RequestData;
                    messageNextRequestData["PodcasterId"] = deleteAccountFavoritedPodcasterChannelsTerminatePodcasterForceParameterDTO.PodcasterId;
                    var messageResponseData = JObject.FromObject(new
                    {
                        // Message = "Delete favorited podcaster channels successfully",
                        PodcasterId = deleteAccountFavoritedPodcasterChannelsTerminatePodcasterForceParameterDTO.PodcasterId,
                    });
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.UserManagementDomain,
                        requestData: messageNextRequestData,
                        responseData: messageResponseData,
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "delete-account-favorited-podcaster-channels-terminate-podcaster-force.success"
                    );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();

                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.UserManagementDomain,
                        requestData: command.RequestData,
                        responseData: JObject.FromObject(new
                        {
                            ErrorMessage = $"Delete account favorited podcaster channels terminate podcaster force failed, error: {ex.Message}"
                        }),
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "delete-account-favorited-podcaster-channels-terminate-podcaster-force.failed"
                    );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);
                    Console.WriteLine("\n" + ex.StackTrace + "\n");
                }
            }
        }

        public async Task DeleteAccountFollowedChannelShowsUnpublishChannelForce(DeleteAccountFollowedChannelShowsUnpublishChannelForceParameterDTO deleteAccountFollowedChannelShowsUnpublishChannelForceParameterDTO, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {

                    // List<int> affectedAccountIds = await _unitOfWork.AccountFollowedPodcastChannelRepository.DeleteByPodcastChannelIdAsync(deleteAccountFollowedChannelShowsUnpublishChannelForceParameterDTO.PodcastChannelId);
                    foreach (var podcastShowId in deleteAccountFollowedChannelShowsUnpublishChannelForceParameterDTO.DmcaDismissedShowIds)
                    {
                        List<int> affectedAccountIds = await _unitOfWork.AccountFollowedPodcastShowRepository.DeleteByPodcastShowIdAsync(podcastShowId);
                    }

                    await transaction.CommitAsync();

                    var messageNextRequestData = command.RequestData;
                    messageNextRequestData["PodcastChannelId"] = deleteAccountFollowedChannelShowsUnpublishChannelForceParameterDTO.PodcastChannelId;
                    messageNextRequestData["DmcaDismissedShowIds"] = JArray.FromObject(deleteAccountFollowedChannelShowsUnpublishChannelForceParameterDTO.DmcaDismissedShowIds);
                    var messageResponseData = JObject.FromObject(new
                    {
                        // Message = "Delete followed channel successfully",
                        PodcastChannelId = deleteAccountFollowedChannelShowsUnpublishChannelForceParameterDTO.PodcastChannelId,
                        DmcaDismissedShowIds = deleteAccountFollowedChannelShowsUnpublishChannelForceParameterDTO.DmcaDismissedShowIds,
                    });
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.UserManagementDomain,
                        requestData: messageNextRequestData,
                        responseData: messageResponseData,
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "delete-account-followed-channel-shows-unpublish-channel-force.success"
                    );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();

                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.UserManagementDomain,
                        requestData: command.RequestData,
                        responseData: JObject.FromObject(new
                        {
                            ErrorMessage = $"Delete account followed channel show unpublish channel force failed, error: {ex.Message}"
                        }),
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "delete-account-followed-channel-shows-unpublish-channel-force.failed"
                    );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);
                    Console.WriteLine("\n" + ex.StackTrace + "\n");
                }
            }
        }

        public async Task DeleteAccountFollowedChannelShowsChannelDeletionForce(DeleteAccountFollowedChannelShowsChannelDeletionForceParameterDTO deleteAccountFollowedChannelShowsChannelDeletionForceParameterDTO, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    // lấy ra tất cả các show của channel để xoá
                    var batchRequest = new BatchQueryRequest
                    {
                        Queries = new List<BatchQueryItem>
                        {
                            new BatchQueryItem
                            {
                                Key = "podcastShow",
                                QueryType = "findall",
                                EntityType = "PodcastShow",
                                Parameters = JObject.FromObject(new
                                {
                                    where = new
                                    {
                                        PodcastChannelId = deleteAccountFollowedChannelShowsChannelDeletionForceParameterDTO.PodcastChannelId
                                    },
                                }),
                            }
                        }
                    };

                    var result = await _httpServiceQueryClient.ExecuteBatchAsync("PodcastService", batchRequest);


                    var podcastShow = result.Results["podcastShow"].ToObject<List<PodcastShowDTO>>();

                    List<Guid> podcastShowIds = podcastShow.Select(ps => ps.Id).ToList();

                    foreach (var podcastShowId in podcastShowIds)
                    {
                        List<int> affectedAccountIds = await _unitOfWork.AccountFollowedPodcastShowRepository.DeleteByPodcastShowIdAsync(podcastShowId);
                    }


                    await transaction.CommitAsync();

                    var messageNextRequestData = command.RequestData;
                    messageNextRequestData["PodcastChannelId"] = deleteAccountFollowedChannelShowsChannelDeletionForceParameterDTO.PodcastChannelId;
                    var messageResponseData = JObject.FromObject(new
                    {
                        // Message = "Delete followed channel successfully",
                        PodcastChannelId = deleteAccountFollowedChannelShowsChannelDeletionForceParameterDTO.PodcastChannelId,
                    });
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.UserManagementDomain,
                        requestData: messageNextRequestData,
                        responseData: messageResponseData,
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "delete-account-followed-channel-shows-channel-deletion-force.success"
                    );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();

                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.UserManagementDomain,
                        requestData: command.RequestData,
                        responseData: JObject.FromObject(new
                        {
                            ErrorMessage = $"Delete account followed channel shows channel deletion force failed, error: {ex.Message}"
                        }),
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "delete-account-followed-channel-shows-channel-deletion-force.failed"
                    );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);
                    Console.WriteLine("\n" + ex.StackTrace + "\n");
                }
            }
        }

        public async Task DeleteAccountFollowedPodcasterShowsTerminatePodcasterForce(DeleteAccountFollowedPodcasterShowsTerminatePodcasterForceParameterDTO deleteAccountFollowedPodcasterShowsTerminatePodcasterForceParameterDTO, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {

                    var batchRequest = new BatchQueryRequest
                    {
                        Queries = new List<BatchQueryItem>
                        {
                            new BatchQueryItem
                            {
                                Key = "podcastShow",
                                QueryType = "findall",
                                EntityType = "PodcastShow",
                                Parameters = JObject.FromObject(new
                                {
                                    where = new
                                    {
                                        PodcasterId = deleteAccountFollowedPodcasterShowsTerminatePodcasterForceParameterDTO.PodcasterId
                                    },
                                }),
                            }
                        }
                    };

                    var result = await _httpServiceQueryClient.ExecuteBatchAsync("PodcastService", batchRequest);


                    var podcastShow = result.Results["podcastShow"].ToObject<List<PodcastShowDTO>>();

                    List<Guid> podcastShowIds = podcastShow.Select(ps => ps.Id).ToList();

                    foreach (var podcastShowId in podcastShowIds)
                    {
                        List<int> affectedAccountIds = await _unitOfWork.AccountFollowedPodcastShowRepository.DeleteByPodcastShowIdAsync(podcastShowId);
                    }
                    await transaction.CommitAsync();

                    var messageNextRequestData = command.RequestData;
                    messageNextRequestData["PodcasterId"] = deleteAccountFollowedPodcasterShowsTerminatePodcasterForceParameterDTO.PodcasterId;
                    var messageResponseData = JObject.FromObject(new
                    {
                        // Message = "Delete followed podcaster shows successfully",
                        PodcasterId = deleteAccountFollowedPodcasterShowsTerminatePodcasterForceParameterDTO.PodcasterId,
                    });
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.UserManagementDomain,
                        requestData: messageNextRequestData,
                        responseData: messageResponseData,
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "delete-account-followed-podcaster-shows-terminate-podcaster-force.success"
                    );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();

                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.UserManagementDomain,
                        requestData: command.RequestData,
                        responseData: JObject.FromObject(new
                        {
                            ErrorMessage = $"Delete account followed podcaster shows terminate podcaster force failed, error: {ex.Message}"
                        }),
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "delete-account-followed-podcaster-shows-terminate-podcaster-force.failed"
                    );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);
                    Console.WriteLine("\n" + ex.StackTrace + "\n");
                }
            }
        }

        public async Task DeleteAccountFollowedShowShowDeletionForce(DeleteAccountFollowedShowShowDeletionForceParameterDTO deleteAccountFollowedShowShowDeletionForceParameterDTO, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {

                    List<int> affectedAccountIds = await _unitOfWork.AccountFollowedPodcastShowRepository.DeleteByPodcastShowIdAsync(deleteAccountFollowedShowShowDeletionForceParameterDTO.PodcastShowId);

                    await transaction.CommitAsync();

                    var messageNextRequestData = command.RequestData;
                    messageNextRequestData["PodcastShowId"] = deleteAccountFollowedShowShowDeletionForceParameterDTO.PodcastShowId;
                    var messageResponseData = JObject.FromObject(new
                    {
                        // Message = "Delete followed show successfully",
                        PodcastShowId = deleteAccountFollowedShowShowDeletionForceParameterDTO.PodcastShowId,
                    });
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.UserManagementDomain,
                        requestData: messageNextRequestData,
                        responseData: messageResponseData,
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "delete-account-followed-show-show-deletion-force.success"
                    );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();

                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.UserManagementDomain,
                        requestData: command.RequestData,
                        responseData: JObject.FromObject(new
                        {
                            ErrorMessage = $"Delete account followed show show deletion force failed, error: {ex.Message}"
                        }),
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "delete-account-followed-show-show-deletion-force.failed"
                    );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);
                    Console.WriteLine("\n" + ex.StackTrace + "\n");
                }
            }
        }

        public async Task DeleteBuddyReviewTerminatePodcasterForce(DeleteBuddyReviewTerminatePodcasterForceParameterDTO deleteBuddyReviewTerminatePodcasterForceParameterDTO, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    List<int> affectedAccountIds = await _unitOfWork.PodcastBuddyReviewRepository.DeleteByPodcasterIdAsync(deleteBuddyReviewTerminatePodcasterForceParameterDTO.PodcasterId);

                    await transaction.CommitAsync();

                    var messageNextRequestData = command.RequestData;
                    messageNextRequestData["PodcasterId"] = deleteBuddyReviewTerminatePodcasterForceParameterDTO.PodcasterId;
                    var messageResponseData = JObject.FromObject(new
                    {
                        // Message = "Delete buddy review terminate podcaster force successfully",
                        PodcasterId = deleteBuddyReviewTerminatePodcasterForceParameterDTO.PodcasterId,
                    });
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.PublicReviewManagementDomain,
                        requestData: messageNextRequestData,
                        responseData: messageResponseData,
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "delete-buddy-review-terminate-podcaster-force.success"
                    );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);

                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.PublicReviewManagementDomain,
                        requestData: command.RequestData,
                        responseData: JObject.FromObject(new
                        {
                            ErrorMessage = $"Delete buddy review terminate podcaster force failed, error: {ex.Message}"
                        }),
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "delete-buddy-review-terminate-podcaster-force.failed"
                    );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);
                    Console.WriteLine("\n" + ex.StackTrace + "\n");
                }
            }
        }

        public async Task DeleteAccountFollowedPodcasterTerminatePodcasterForce(DeleteAccountFollowedPodcasterTerminatePodcasterForceParameterDTO deleteAccountFollowedPodcasterTerminatePodcasterForceParameterDTO, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    List<int> affectedAccountIds = await _unitOfWork.AccountFollowedPodcasterRepository.DeleteByPodcasterIdAsync(deleteAccountFollowedPodcasterTerminatePodcasterForceParameterDTO.PodcasterId);

                    await transaction.CommitAsync();

                    var messageNextRequestData = command.RequestData;
                    messageNextRequestData["PodcasterId"] = deleteAccountFollowedPodcasterTerminatePodcasterForceParameterDTO.PodcasterId;
                    var messageResponseData = JObject.FromObject(new
                    {
                        // Message = "Delete followed podcaster terminate podcaster force successfully",
                        PodcasterId = deleteAccountFollowedPodcasterTerminatePodcasterForceParameterDTO.PodcasterId,
                    });
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.UserManagementDomain,
                        requestData: messageNextRequestData,
                        responseData: messageResponseData,
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "delete-account-followed-podcaster-terminate-podcaster-force.success"
                    );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();

                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.UserManagementDomain,
                        requestData: command.RequestData,
                        responseData: JObject.FromObject(new
                        {
                            ErrorMessage = $"Delete account followed podcaster terminate podcaster force failed, error: {ex.Message}"
                        }),
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "delete-account-followed-podcaster-terminate-podcaster-force.failed"
                    );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);
                    Console.WriteLine("\n" + ex.StackTrace + "\n");
                }
            }
        }
    }
}