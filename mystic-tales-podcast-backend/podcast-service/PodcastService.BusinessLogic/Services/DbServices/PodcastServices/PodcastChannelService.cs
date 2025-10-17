using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PodcastService.Common.AppConfigurations.App.interfaces;
using PodcastService.Common.AppConfigurations.FilePath.interfaces;
using PodcastService.DataAccess.Data;
using PodcastService.DataAccess.UOW;
using PodcastService.DataAccess.Repositories.interfaces;
using PodcastService.Common.AppConfigurations.BusinessSetting.interfaces;
using PodcastService.Infrastructure.Services.Google.Email;
using PodcastService.Infrastructure.Configurations.Google.interfaces;
using PodcastService.BusinessLogic.Helpers.AuthHelpers;
using PodcastService.BusinessLogic.Helpers.FileHelpers;
using PodcastService.BusinessLogic.Helpers.DateHelpers;
using PodcastService.BusinessLogic.Services.CrossServiceServices.QueryServices;
using PodcastService.BusinessLogic.Models.CrossService;
using Newtonsoft.Json.Linq;
using PodcastService.Infrastructure.Services.Kafka;
using PodcastService.BusinessLogic.Enums.Kafka;
using PodcastService.Infrastructure.Models.Kafka;
using PodcastService.BusinessLogic.Services.MessagingServices.interfaces;
using PodcastService.Infrastructure.Services.Redis;
using PodcastService.DataAccess.Entities.SqlServer;
using PodcastService.BusinessLogic.DTOs.MessageQueue.ContentManagementDomain.CreateChannel;
using PodcastService.BusinessLogic.Enums.Podcast;
using PodcastService.BusinessLogic.DTOs.Channel.ListItems;
using PodcastService.BusinessLogic.DTOs.Cache;
using PodcastService.BusinessLogic.DTOs.Channel;
using PodcastService.BusinessLogic.DTOs.Channel.Details;
using PodcastService.BusinessLogic.DTOs.Cachegory;
using PodcastService.BusinessLogic.DTOs.Hashtag;
using PodcastService.BusinessLogic.DTOs.Subscription.ListItems;
using PodcastService.BusinessLogic.DTOs.Subscription;
using PodcastService.BusinessLogic.DTOs.Show.ListItems;
using PodcastService.BusinessLogic.DTOs.Account;
using PodcastService.BusinessLogic.Services.DbServices.MiscServices;
using PodcastService.BusinessLogic.DTOs.Show;
using PodcastService.BusinessLogic.DTOs.MessageQueue.ContentManagementDomain.UpdateChannel;
using PodcastService.BusinessLogic.DTOs.MessageQueue.ContentManagementDomain.PublishChannel;

namespace PodcastService.BusinessLogic.Services.DbServices.PodcastServices
{
    public class PodcastChannelService
    {
        // LOGGER
        private readonly ILogger<PodcastChannelService> _logger;

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
        private readonly IGenericRepository<PodcastChannel> _podcastChannelGenericRepository;
        private readonly IGenericRepository<PodcastChannelStatusTracking> _podcastChannelStatusTrackingGenericRepository;
        private readonly IGenericRepository<PodcastChannelHashtag> _podcastChannelHashtagGenericRepository;
        private readonly IGenericRepository<Hashtag> _hashtagGenericRepository;
        private readonly IGenericRepository<PodcastShow> _podcastShowGenericRepository;


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

        public PodcastChannelService(
            ILogger<PodcastChannelService> logger,
            AppDbContext appDbContext,
            BcryptHelper bcryptHelper,
            FluentEmailService fluentEmailService,
            JwtHelper jwtHelper,
            IUnitOfWork unitOfWork,

            IServiceProvider serviceProvider,
            IGenericRepository<PodcastChannel> podcastChannelGenericRepository,
            IGenericRepository<PodcastChannelStatusTracking> podcastChannelStatusTrackingGenericRepository,
            IGenericRepository<PodcastChannelHashtag> podcastChannelHashtagGenericRepository,
            IGenericRepository<Hashtag> hashtagGenericRepository,
            IGenericRepository<PodcastShow> podcastShowGenericRepository,

            FileIOHelper fileIOHelper,
            DateHelper dateHelper,

            IFilePathConfig filePathConfig,
            IGoogleMailConfig googleMailConfig,
            IAppConfig appConfig,
            IAccountConfig accountConfig,

            HttpServiceQueryClient httpServiceQueryClient,

            AccountCachingService accountCachingService,

            IMessagingService messagingService,
            KafkaProducerService kafkaProducerService,

            RedisSharedCacheService redisSharedCacheService
            )
        {
            _logger = logger;

            _appDbContext = appDbContext;
            _unitOfWork = unitOfWork;

            _podcastChannelGenericRepository = podcastChannelGenericRepository;
            _podcastChannelStatusTrackingGenericRepository = podcastChannelStatusTrackingGenericRepository;
            _podcastChannelHashtagGenericRepository = podcastChannelHashtagGenericRepository;
            _hashtagGenericRepository = hashtagGenericRepository;
            _podcastShowGenericRepository = podcastShowGenericRepository;

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

            _accountCachingService = accountCachingService;

            _messagingService = messagingService;
            _kafkaProducerService = kafkaProducerService;

            _redisSharedCacheService = redisSharedCacheService;
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

        public async Task<AccountStatusCache> QueryAccountStatusCacheById(int accountId)
        {
            var batchRequest = new BatchQueryRequest
            {
                Queries = new List<BatchQueryItem>
                {
                    new BatchQueryItem
                    {
                        Key = "account",
                        QueryType = "findbyid",
                        EntityType = "Account",
                        Parameters = JObject.FromObject(new
                        {
                            id = accountId,
                            include = "PodcasterProfile"
                        }),
                        Fields = new[] {
                            "Id",
                            "Email",
                            "Password",
                            "RoleId",
                            "FullName",
                            "Dob",
                            "Gender",
                            "Address",
                            "Phone",
                            "Balance",
                            "MainImageFileKey",
                            "IsVerified",
                            "GoogleId",
                            "VerifyCode",
                            "PodcastListenSlot",
                            "ViolationPoint",
                            "ViolationLevel",
                            "LastViolationPointChanged",
                            "LastViolationLevelChanged",
                            "LastPodcastListenSlotChanged",
                            "DeactivatedAt",
                            "CreatedAt",
                            "UpdatedAt",
                            "PodcasterProfile",
                        }
                    }
                }
            };
            var result = await _httpServiceQueryClient.ExecuteBatchAsync("UserService", batchRequest);
            if (result.Results["account"] == null) return null;


            var accountStatusCache = (result.Results["account"] as JObject).ToObject<AccountStatusCache>();
            var podcasterProfile = result.Results["account"]["PodcasterProfile"] as JObject;
            accountStatusCache.HasVerifiedPodcasterProfile = accountStatusCache.RoleId == 1 && podcasterProfile != null && podcasterProfile["IsVerified"]?.ToObject<bool>() == true ? true : false;
            Console.WriteLine($"Queried Account: Id={accountStatusCache.Id}, RoleId={accountStatusCache.RoleId}, IsVerified={accountStatusCache.IsVerified}, DeactivatedAt={accountStatusCache.DeactivatedAt}, HasVerifiedPodcasterProfile={accountStatusCache.HasVerifiedPodcasterProfile}");
            return accountStatusCache;
        }


        /////////////////////////////////////////////////////////////

        public async Task SendPodcastServiceEmail(MailProperty mailProperty, string toEmail, object viewModel)
        {
            try
            {
                await _fluentEmailService.SendEmail(toEmail, viewModel, mailProperty.TemplateFilePath
                , mailProperty.Subject);
            }
            catch (Exception ex)
            {
                throw new HttpRequestException("Send email failed, error: " + ex.Message);
            }
        }

        #region Sample coding format must be followed
        // public async Task RegisterAccount(CreateAccountParameterDTO accountRegisterDTO, SagaCommandMessage command)
        // {
        //     var registerInfo = accountRegisterDTO;
        //     using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
        //     {
        //         try
        //         {
        //             var existAccount = await _unitOfWork.AccountRepository.FindByEmailAsync(registerInfo.Email);

        //             var activeSystemConfigProfile = await GetActiveSystemConfigProfile();

        //             if (existAccount != null)
        //             {
        //                 if (existAccount.IsVerified == true)
        //                 {
        //                     throw new Exception("Account with email " + registerInfo.Email + " already exists and is verified.");
        //                 }
        //                 else
        //                 {
        //                     string verifyCode = GenerateRandomVerifyCode(_accountConfig.VerifyCodeLength);

        //                     existAccount.Email = registerInfo.Email;
        //                     existAccount.Password = _bcryptHelper.HashPassword(registerInfo.Password);
        //                     existAccount.FullName = registerInfo.FullName;
        //                     existAccount.RoleId = registerInfo.RoleId;
        //                     existAccount.Dob = DateOnly.FromDateTime(registerInfo.Dob);
        //                     existAccount.Gender = registerInfo.Gender;
        //                     existAccount.Address = registerInfo.Address;
        //                     existAccount.Phone = registerInfo.Phone;
        //                     existAccount.IsVerified = registerInfo.RoleId == 1 ? false : true;
        //                     existAccount.VerifyCode = registerInfo.RoleId == 1 ? verifyCode : null;
        //                     existAccount.PodcastListenSlot = registerInfo.RoleId == 1 ? activeSystemConfigProfile["AccountConfig"].Value<int?>("PodcastListenSlotThreshold") : null;
        //                     existAccount.MainImageFileKey = null;
        //                     // await _fluentEmailService.SendEmail(registerInfo.Email, new VerifyCodeEmailViewModel
        //                     // {
        //                     //     Email = registerInfo.Email,
        //                     //     FullName = registerInfo.FullName,
        //                     //     VerifyCode = verifyCode
        //                     // }, _googleMailConfig.AccountVerification_TemplateViewPath
        //                     // , _googleMailConfig.AccountVerification_MailSubject);

        //                     if (registerInfo.RoleId == 1)
        //                     {
        //                         var mailSendingRequestData = JObject.FromObject(new
        //                         {
        //                             SendPodcastServiceEmailMailInfo = new
        //                             {
        //                                 MailTypeName = "CustomerRegistrationVerification",
        //                                 ToEmail = registerInfo.Email,
        //                                 MailObject = new CustomerRegistrationVerificationMailViewModel
        //                                 {
        //                                     Email = registerInfo.Email,
        //                                     FullName = registerInfo.FullName,
        //                                     VerifyCode = verifyCode
        //                                 }
        //                             }
        //                         });
        //                         var mailSendingFlow = _kafkaProducerService.PrepareStartSagaTriggerMessage(
        //                             topic: KafkaTopicEnum.UserManagementDomain,
        //                             requestData: mailSendingRequestData,
        //                             sagaInstanceId: null,
        //                             messageName: "user-service-mail-sending-flow");
        //                         await _messagingService.SendSagaMessageAsync(mailSendingFlow);
        //                     }


        //                     await _accountGenericRepository.UpdateAsync(existAccount.Id, existAccount);

        //                 }
        //             }
        //             else
        //             {
        //                 string verifyCode = GenerateRandomVerifyCode(_accountConfig.VerifyCodeLength);

        //                 existAccount = new Account
        //                 {
        //                     Email = registerInfo.Email,
        //                     Password = _bcryptHelper.HashPassword(registerInfo.Password),
        //                     FullName = registerInfo.FullName,
        //                     RoleId = registerInfo.RoleId,
        //                     Dob = DateOnly.FromDateTime(registerInfo.Dob),
        //                     Gender = registerInfo.Gender,
        //                     Address = registerInfo.Address,
        //                     Phone = registerInfo.Phone,
        //                     IsVerified = registerInfo.RoleId == 1 ? false : true,
        //                     VerifyCode = registerInfo.RoleId == 1 ? verifyCode : null,
        //                     PodcastListenSlot = registerInfo.RoleId == 1 ? activeSystemConfigProfile["AccountConfig"].Value<int?>("PodcastListenSlotThreshold") : null,
        //                     MainImageFileKey = null,
        //                 };

        //                 // await _fluentEmailService.SendEmail(registerInfo.Email, new VerifyCodeEmailViewModel
        //                 // {
        //                 //     Email = registerInfo.Email,
        //                 //     FullName = registerInfo.FullName,
        //                 //     VerifyCode = verifyCode
        //                 // }, _googleMailConfig.AccountVerification_TemplateViewPath
        //                 // , _googleMailConfig.AccountVerification_MailSubject);

        //                 if (registerInfo.RoleId == 1)
        //                 {
        //                     // var mailSendingRequestData = JObject.FromObject(new
        //                     // {
        //                     //     MailTypeName = "CustomerRegistrationVerification",
        //                     //     ToEmail = registerInfo.Email,
        //                     //     MailObject = new CustomerRegistrationVerificationMailViewModel
        //                     //     {
        //                     //         Email = registerInfo.Email,
        //                     //         FullName = registerInfo.FullName,
        //                     //         VerifyCode = verifyCode
        //                     //     }
        //                     // });
        //                     var mailSendingRequestData = JObject.FromObject(new
        //                     {
        //                         SendPodcastServiceEmailMailInfo = new
        //                         {
        //                             MailTypeName = "CustomerRegistrationVerification",
        //                             ToEmail = registerInfo.Email,
        //                             MailObject = new CustomerRegistrationVerificationMailViewModel
        //                             {
        //                                 Email = registerInfo.Email,
        //                                 FullName = registerInfo.FullName,
        //                                 VerifyCode = verifyCode
        //                             }
        //                         }
        //                     });
        //                     var mailSendingFlow = _kafkaProducerService.PrepareStartSagaTriggerMessage(
        //                             topic: KafkaTopicEnum.UserManagementDomain,
        //                             requestData: mailSendingRequestData,
        //                             sagaInstanceId: null,
        //                             messageName: "user-service-mail-sending-flow");
        //                     await _messagingService.SendSagaMessageAsync(mailSendingFlow);
        //                 }

        //                 await _accountGenericRepository.CreateAsync(existAccount);
        //             }



        //             var folderPath = _filePathConfig.ACCOUNT_FILE_PATH + "\\" + existAccount.Id;
        //             if (registerInfo.MainImageFileKey != null && registerInfo.MainImageFileKey != "")
        //             {
        //                 var MainImageFileKey = FilePathHelper.CombinePaths(folderPath, $"main_image{FilePathHelper.GetExtension(registerInfo.MainImageFileKey)}");
        //                 await _fileIOHelper.CopyFileToFileAsync(registerInfo.MainImageFileKey, MainImageFileKey);
        //                 await _fileIOHelper.DeleteFileAsync(registerInfo.MainImageFileKey);
        //                 existAccount.MainImageFileKey = MainImageFileKey;
        //                 await _accountGenericRepository.UpdateAsync(existAccount.Id, existAccount);
        //             }

        //             await transaction.CommitAsync();

        //             // var messageNextRequestData = JObject.FromObject(new
        //             // {
        //             //     Email = existAccount.Email,
        //             //     FullName = existAccount.FullName,
        //             //     Dob = existAccount.Dob?.ToString("yyyy-MM-dd"),
        //             //     Gender = existAccount.Gender,
        //             //     Address = existAccount.Address,
        //             //     Phone = existAccount.Phone,
        //             //     MainImageFileKey = existAccount.MainImageFileKey,
        //             //     RoleId = existAccount.RoleId,
        //             //     Password = existAccount.Password,
        //             // });
        //             var messageNextRequestData = command.RequestData;
        //             messageNextRequestData["Email"] = existAccount.Email;
        //             messageNextRequestData["FullName"] = existAccount.FullName;
        //             messageNextRequestData["Dob"] = existAccount.Dob?.ToString("yyyy-MM-dd");
        //             messageNextRequestData["Gender"] = existAccount.Gender;
        //             messageNextRequestData["Address"] = existAccount.Address;
        //             messageNextRequestData["Phone"] = existAccount.Phone;
        //             messageNextRequestData["MainImageFileKey"] = existAccount.MainImageFileKey;
        //             messageNextRequestData["RoleId"] = existAccount.RoleId;
        //             messageNextRequestData["Password"] = existAccount.Password;

        //             var messageResponseData = JObject.FromObject(new
        //             {
        //                 AccountId = existAccount.Id,
        //                 // Message = "Register account successfully"
        //                 VerifyCode = existAccount.VerifyCode,
        //                 Email = existAccount.Email
        //             });
        //             var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
        //                 topic: KafkaTopicEnum.UserManagementDomain,
        //                 requestData: messageNextRequestData,
        //                 responseData: messageResponseData,
        //                 sagaInstanceId: command.SagaInstanceId,
        //                 flowName: command.FlowName,
        //                 messageName: "create-account.success"
        //                 );
        //             await _messagingService.SendSagaMessageAsync(sagaEventMessage);
        //             await SendChangeAccountStatusMessage(existAccount.Id);
        //         }
        //         catch (Exception ex)
        //         {
        //             await transaction.RollbackAsync();

        //             // xoá file tạm
        //             if (registerInfo.MainImageFileKey != null && registerInfo.MainImageFileKey != "")
        //             {
        //                 await _fileIOHelper.DeleteFileAsync(registerInfo.MainImageFileKey);
        //             }

        //             var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
        //                 topic: KafkaTopicEnum.UserManagementDomain,
        //                 requestData: command.RequestData,
        //                 responseData: JObject.FromObject(new
        //                 {
        //                     ErrorMessage = $"Account registration failed, error: {ex.Message}"
        //                 }),
        //                 sagaInstanceId: command.SagaInstanceId,
        //                 flowName: command.FlowName,
        //                 messageName: "create-account.failed"
        //                 );
        //             await _messagingService.SendSagaMessageAsync(sagaEventMessage);

        //             Console.WriteLine("\n" + ex.StackTrace + "\n");
        //             // throw new HttpRequestException("Đăng kí tài khoản thất bại, lỗi: " + ex.Message);
        //         }
        //     }

        // }

        // public async Task<List<AccountListItemResponseDTO>> GetCustomerAccounts()
        // {
        //     try
        //     {
        //         var customers = await _unitOfWork.AccountRepository.FindByRoleIdAsync(1, null, a => a.Include(ac => ac.Role));

        //         var result = customers.Select(item =>
        //         {
        //             return new AccountListItemResponseDTO
        //             {
        //                 Id = item.Id,
        //                 Email = item.Email,
        //                 Role = new RoleDTO
        //                 {
        //                     Id = item.Role.Id,
        //                     Name = item.Role.Name
        //                 },
        //                 FullName = item.FullName,
        //                 Dob = item.Dob?.ToString("yyyy-MM-dd"),
        //                 Gender = item.Gender,
        //                 Address = item.Address,
        //                 Phone = item.Phone,
        //                 Balance = item.Balance,
        //                 IsVerified = item.IsVerified,
        //                 PodcastListenSlot = item.PodcastListenSlot,
        //                 ViolationPoint = item.ViolationPoint,
        //                 ViolationLevel = item.ViolationLevel,
        //                 LastPodcastListenSlotChanged = item.LastPodcastListenSlotChanged?.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
        //                 LastViolationPointChanged = item.LastViolationPointChanged?.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
        //                 LastViolationLevelChanged = item.LastViolationLevelChanged?.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
        //                 MainImageFileKey = item.MainImageFileKey,
        //                 DeactivatedAt = item.DeactivatedAt?.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
        //                 CreatedAt = item.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
        //                 UpdatedAt = item.UpdatedAt.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),

        //             };
        //         });

        //         return result.ToList();
        //     }
        //     catch (Exception ex)
        //     {
        //         Console.WriteLine("\n" + ex.StackTrace + "\n");
        //         // throw new HttpRequestException("Lấy danh sách tài khoản khách hàng thất bại, lỗi: " + ex.Message);
        //         throw new Exception("Get customer account list failed, error: " + ex.Message);
        //     }

        // }

        // public async Task<List<AccountListItemResponseDTO>> GetStaffAccounts(bool? IsDeactivated = null)
        // {
        //     try
        //     {
        //         List<int> roles = new List<int> { 2 }; // 2: Head, 3: Assignee
        //         var staffs = await _unitOfWork.AccountRepository.FindByRoleIdsAsync(roles,
        //             predicate: IsDeactivated.HasValue ? (a => (IsDeactivated == true ? a.DeactivatedAt != null : a.DeactivatedAt == null)) : null
        //         , a => a.Include(ac => ac.Role));

        //         var result = staffs.Select(item =>
        //         {
        //             return new AccountListItemResponseDTO
        //             {
        //                 Id = item.Id,
        //                 Email = item.Email,
        //                 Role = new RoleDTO
        //                 {
        //                     Id = item.Role.Id,
        //                     Name = item.Role.Name
        //                 },
        //                 FullName = item.FullName,
        //                 Dob = item.Dob?.ToString("yyyy-MM-dd"),
        //                 Gender = item.Gender,
        //                 Address = item.Address,
        //                 Phone = item.Phone,
        //                 Balance = item.Balance,
        //                 IsVerified = item.IsVerified,
        //                 PodcastListenSlot = item.PodcastListenSlot,
        //                 ViolationPoint = item.ViolationPoint,
        //                 ViolationLevel = item.ViolationLevel,
        //                 LastPodcastListenSlotChanged = item.LastPodcastListenSlotChanged?.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
        //                 LastViolationPointChanged = item.LastViolationPointChanged?.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
        //                 LastViolationLevelChanged = item.LastViolationLevelChanged?.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
        //                 MainImageFileKey = item.MainImageFileKey,
        //                 DeactivatedAt = item.DeactivatedAt?.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
        //                 CreatedAt = item.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
        //                 UpdatedAt = item.UpdatedAt.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
        //             };
        //         });

        //         return result.ToList();
        //     }
        //     catch (Exception ex)
        //     {
        //         Console.WriteLine("\n" + ex.StackTrace + "\n");
        //         throw new HttpRequestException("Get staff account list failed, error: " + ex.Message);
        //     }

        // }

        // public async Task<List<PodcasterListItemResponseDTO>> GetPodcasterAccounts()
        // {
        //     try
        //     {

        //         var podcasters = await _unitOfWork.AccountRepository.FindByRoleIdAsync(1,
        //             predicate: a => a.PodcasterProfile != null,
        //                 a => a.Include(ac => ac.Role)
        //                         .Include(ac => ac.PodcasterProfile)  // Expression riêng biệt
        //                         .Include(ac => ac.PodcastBuddyReviewPodcastBuddies)
        //                         .ThenInclude(r => r.Account)
        //             );


        //         var result = podcasters.Select(item =>
        //         {
        //             Console.WriteLine("Số podcaster tìm thấy: " + (item.PodcastBuddyReviewPodcastBuddies.Count > 0 ? item.PodcastBuddyReviewPodcastBuddies.Count : 0));

        //             return new PodcasterListItemResponseDTO
        //             {
        //                 Id = item.Id,
        //                 Email = item.Email,
        //                 Role = new RoleDTO
        //                 {
        //                     Id = item.Role.Id,
        //                     Name = item.Role.Name
        //                 },
        //                 FullName = item.FullName,
        //                 Dob = item.Dob?.ToString("yyyy-MM-dd"),
        //                 Gender = item.Gender,
        //                 PodcastListenSlot = item.PodcastListenSlot,
        //                 ViolationPoint = item.ViolationPoint,
        //                 ViolationLevel = item.ViolationLevel,
        //                 LastPodcastListenSlotChanged = item.LastPodcastListenSlotChanged?.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
        //                 LastViolationPointChanged = item.LastViolationPointChanged?.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
        //                 LastViolationLevelChanged = item.LastViolationLevelChanged?.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
        //                 Address = item.Address,
        //                 Phone = item.Phone,
        //                 Balance = item.Balance,
        //                 IsVerified = item.IsVerified,
        //                 MainImageFileKey = item.MainImageFileKey,
        //                 DeactivatedAt = item.DeactivatedAt?.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
        //                 CreatedAt = item.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
        //                 UpdatedAt = item.UpdatedAt.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
        //                 PodcasterProfile = new PodcasterProfileDTO
        //                 {
        //                     AccountId = item.PodcasterProfile.AccountId,
        //                     Name = item.PodcasterProfile.Name,
        //                     Description = item.PodcasterProfile.Description,
        //                     AverageRating = item.PodcasterProfile.AverageRating,
        //                     RatingCount = item.PodcasterProfile.RatingCount,
        //                     TotalFollow = item.PodcasterProfile.TotalFollow,
        //                     CommitmentDocumentFileKey = item.PodcasterProfile.CommitmentDocumentFileKey,
        //                     BuddyAudioFileKey = item.PodcasterProfile.BuddyAudioFileKey,
        //                     OwnedBookingStorageSize = item.PodcasterProfile.OwnedBookingStorageSize,
        //                     UsedBookingStorageSize = item.PodcasterProfile.UsedBookingStorageSize,
        //                     IsVerified = item.PodcasterProfile.IsVerified,
        //                     CreatedAt = item.PodcasterProfile.CreatedAt,
        //                     UpdatedAt = item.PodcasterProfile.UpdatedAt,
        //                 },
        //                 ReviewList = item.PodcastBuddyReviewPodcastBuddies?
        //                 .Where(r => r.Account != null).Select(r => new ReviewListItemDTO
        //                 {
        //                     Id = r.Id,
        //                     Account = new AccountSnippetDTO
        //                     {
        //                         Id = r.Account.Id,
        //                         FullName = r.Account.FullName,
        //                         Email = r.Account.Email,
        //                         MainImageFileKey = r.Account.MainImageFileKey
        //                     },
        //                     Rating = r.Rating,
        //                     Content = r.Content,
        //                     DeletedAt = r.DeletedAt,
        //                     PodcastBuddyId = r.PodcastBuddyId,
        //                     Title = r.Title,
        //                     UpdatedAt = r.UpdatedAt
        //                 }).ToList()
        //             };
        //         });
        //         return result.ToList();
        //     }
        //     catch (Exception ex)
        //     {
        //         Console.WriteLine("\n" + ex.StackTrace + "\n");
        //         throw new HttpRequestException("Get podcaster account list failed, error: " + ex.Message);
        //     }
        // }

        // public async Task<List<PodcastBuddyListItemResponseDTO>> GetPodcastBuddyAccounts(int? requestRoleId)
        // {
        //     try
        //     {

        //         var podcasters = await _unitOfWork.AccountRepository.FindByRoleIdAsync(1,
        //             predicate: a => a.PodcasterProfile != null && a.PodcasterProfile.IsVerified == true && a.IsVerified == true,
        //                 a => a.Include(ac => ac.Role)
        //                         .Include(ac => ac.PodcasterProfile)  // Expression riêng biệt
        //                         .Include(ac => ac.PodcastBuddyReviewPodcastBuddies)
        //                         .ThenInclude(r => r.Account)
        //             );

        //         // nếu requestRoleId là 1 thì loại bỏ các account có DeactivatedAt khác null và violation level != 0
        //         if (requestRoleId == null || requestRoleId == 1)
        //         {
        //             podcasters = podcasters.Where(p => p.DeactivatedAt == null && (p.ViolationLevel == 0 || p.ViolationLevel == null)).ToList();
        //         }



        //         var result = podcasters.Select(item =>
        //         {
        //             Console.WriteLine("Số podcaster tìm thấy: " + (item.PodcastBuddyReviewPodcastBuddies.Count > 0 ? item.PodcastBuddyReviewPodcastBuddies.Count : 0));

        //             return new PodcastBuddyListItemResponseDTO
        //             {
        //                 PodcastBuddyProfile = new PodcastBuddyProfileDTO
        //                 {
        //                     AccountId = item.PodcasterProfile.AccountId,
        //                     Name = item.PodcasterProfile.Name,
        //                     Description = item.PodcasterProfile.Description,
        //                     AverageRating = item.PodcasterProfile.AverageRating,
        //                     RatingCount = item.PodcasterProfile.RatingCount,
        //                     TotalFollow = item.PodcasterProfile.TotalFollow,
        //                     CommitmentDocumentFileKey = item.PodcasterProfile.CommitmentDocumentFileKey,
        //                     BuddyAudioFileKey = item.PodcasterProfile.BuddyAudioFileKey,
        //                     IsVerified = item.PodcasterProfile.IsVerified,
        //                 },
        //                 ReviewList = item.PodcastBuddyReviewPodcastBuddies?
        //                 .Where(r => r.Account != null).Select(r => new ReviewListItemDTO
        //                 {
        //                     Id = r.Id,
        //                     Account = new AccountSnippetDTO
        //                     {
        //                         Id = r.Account.Id,
        //                         FullName = r.Account.FullName,
        //                         Email = r.Account.Email,
        //                         MainImageFileKey = r.Account.MainImageFileKey
        //                     },
        //                     Rating = r.Rating,
        //                     Content = r.Content,
        //                     DeletedAt = r.DeletedAt,
        //                     PodcastBuddyId = r.PodcastBuddyId,
        //                     Title = r.Title,
        //                     UpdatedAt = r.UpdatedAt
        //                 }).ToList()
        //             };
        //         });
        //         return result.ToList();
        //     }
        //     catch (Exception ex)
        //     {
        //         Console.WriteLine("\n" + ex.StackTrace + "\n");
        //         throw new HttpRequestException("Get podcast buddy account list failed, error: " + ex.Message);
        //     }
        // }

        // public async Task<PodcasterListItemResponseDTO> GetPodcasterProfileByAccountId(int accountId)
        // {
        //     try
        //     {
        //         var podcaster = (await _unitOfWork.AccountRepository.FindByRoleIdAsync(1,
        //             predicate: a => a.PodcasterProfile != null && a.PodcasterProfile.AccountId == accountId,
        //                 a => a.Include(ac => ac.Role)
        //                         .Include(ac => ac.PodcasterProfile)  // Expression riêng biệt
        //                         .Include(ac => ac.PodcastBuddyReviewPodcastBuddies)
        //                         .ThenInclude(r => r.Account)
        //             )).FirstOrDefault();

        //         // đếm số review
        //         if (podcaster == null)
        //         {
        //             throw new Exception("Podcaster account with id " + accountId + " not found");
        //         }

        //         Console.WriteLine("Số podcaster tìm thấy: " + (podcaster.PodcastBuddyReviewPodcastBuddies.Count > 0 ? podcaster.PodcastBuddyReviewPodcastBuddies.Count : 0));

        //         return new PodcasterListItemResponseDTO
        //         {
        //             Id = podcaster.Id,
        //             Email = podcaster.Email,
        //             Role = new RoleDTO
        //             {
        //                 Id = podcaster.Role.Id,
        //                 Name = podcaster.Role.Name
        //             },
        //             FullName = podcaster.FullName,
        //             Dob = podcaster.Dob?.ToString("yyyy-MM-dd"),
        //             Gender = podcaster.Gender,
        //             PodcastListenSlot = podcaster.PodcastListenSlot,
        //             ViolationPoint = podcaster.ViolationPoint,
        //             ViolationLevel = podcaster.ViolationLevel,
        //             LastPodcastListenSlotChanged = podcaster.LastPodcastListenSlotChanged?.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
        //             LastViolationPointChanged = podcaster.LastViolationPointChanged?.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
        //             LastViolationLevelChanged = podcaster.LastViolationLevelChanged?.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
        //             Address = podcaster.Address,
        //             Phone = podcaster.Phone,
        //             Balance = podcaster.Balance,
        //             IsVerified = podcaster.IsVerified,
        //             MainImageFileKey = podcaster.MainImageFileKey,
        //             DeactivatedAt = podcaster.DeactivatedAt?.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
        //             CreatedAt = podcaster.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
        //             UpdatedAt = podcaster.UpdatedAt.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
        //             PodcasterProfile = new PodcasterProfileDTO
        //             {
        //                 AccountId = podcaster.PodcasterProfile.AccountId,
        //                 Name = podcaster.PodcasterProfile.Name,
        //                 Description = podcaster.PodcasterProfile.Description,
        //                 AverageRating = podcaster.PodcasterProfile.AverageRating,
        //                 RatingCount = podcaster.PodcasterProfile.RatingCount,
        //                 TotalFollow = podcaster.PodcasterProfile.TotalFollow,
        //                 CommitmentDocumentFileKey = podcaster.PodcasterProfile.CommitmentDocumentFileKey,
        //                 BuddyAudioFileKey = podcaster.PodcasterProfile.BuddyAudioFileKey,
        //                 OwnedBookingStorageSize = podcaster.PodcasterProfile.OwnedBookingStorageSize,
        //                 UsedBookingStorageSize = podcaster.PodcasterProfile.UsedBookingStorageSize,
        //                 IsVerified = podcaster.PodcasterProfile.IsVerified,
        //                 CreatedAt = podcaster.PodcasterProfile.CreatedAt,
        //                 UpdatedAt = podcaster.PodcasterProfile.UpdatedAt,
        //             },
        //             ReviewList = podcaster.PodcastBuddyReviewPodcastBuddies?
        //             .Where(r => r.Account != null).Select(r => new ReviewListItemDTO
        //             {
        //                 Id = r.Id,
        //                 Account = new AccountSnippetDTO
        //                 {
        //                     Id = r.Account.Id,
        //                     FullName = r.Account.FullName,
        //                     Email = r.Account.Email,
        //                     MainImageFileKey = r.Account.MainImageFileKey
        //                 },
        //                 Rating = r.Rating,
        //                 Content = r.Content,
        //                 DeletedAt = r.DeletedAt,
        //                 PodcastBuddyId = r.PodcastBuddyId,
        //                 Title = r.Title,
        //                 UpdatedAt = r.UpdatedAt
        //             }).ToList()
        //         };

        //     }
        //     catch (Exception ex)
        //     {
        //         Console.WriteLine("\n" + ex.StackTrace + "\n");
        //         throw new HttpRequestException("Get podcaster account list failed, error: " + ex.Message);
        //     }
        // }

        // public async Task<PodcastBuddyListItemResponseDTO> GetPodcastBuddyProfileByAccountId(int accountId, int requestRoleId)
        // {
        //     try
        //     {
        //         var podcaster = (await _unitOfWork.AccountRepository.FindByRoleIdAsync(1,
        //             predicate: a => a.PodcasterProfile != null && a.PodcasterProfile.AccountId == accountId && a.PodcasterProfile.IsVerified == true && a.IsVerified == true,
        //                 a => a.Include(ac => ac.Role)
        //                         .Include(ac => ac.PodcasterProfile)  // Expression riêng biệt
        //                         .Include(ac => ac.PodcastBuddyReviewPodcastBuddies)
        //                         .ThenInclude(r => r.Account)
        //             )).FirstOrDefault();

        //         // đếm số review
        //         if (podcaster == null)
        //         {
        //             throw new Exception("Podcaster account with id " + accountId + " not found");
        //         }

        //         // nếu requestRoleId là 1 thì loại bỏ các account có DeactivatedAt khác null và violation level != 0
        //         if (requestRoleId == 1 && (podcaster.DeactivatedAt != null || podcaster.ViolationLevel != 0))
        //         {
        //             throw new Exception("Podcaster account with id " + accountId + " not found");
        //         }




        //         Console.WriteLine("Số podcaster tìm thấy: " + (podcaster.PodcastBuddyReviewPodcastBuddies.Count > 0 ? podcaster.PodcastBuddyReviewPodcastBuddies.Count : 0));

        //         return new PodcastBuddyListItemResponseDTO
        //         {
        //             PodcastBuddyProfile = new PodcastBuddyProfileDTO
        //             {
        //                 AccountId = podcaster.PodcasterProfile.AccountId,
        //                 Name = podcaster.PodcasterProfile.Name,
        //                 Description = podcaster.PodcasterProfile.Description,
        //                 AverageRating = podcaster.PodcasterProfile.AverageRating,
        //                 RatingCount = podcaster.PodcasterProfile.RatingCount,
        //                 TotalFollow = podcaster.PodcasterProfile.TotalFollow,
        //                 CommitmentDocumentFileKey = podcaster.PodcasterProfile.CommitmentDocumentFileKey,
        //                 BuddyAudioFileKey = podcaster.PodcasterProfile.BuddyAudioFileKey,
        //                 IsVerified = podcaster.PodcasterProfile.IsVerified,
        //             },
        //             ReviewList = podcaster.PodcastBuddyReviewPodcastBuddies?
        //             .Where(r => r.Account != null).Select(r => new ReviewListItemDTO
        //             {
        //                 Id = r.Id,
        //                 Account = new AccountSnippetDTO
        //                 {
        //                     Id = r.Account.Id,
        //                     FullName = r.Account.FullName,
        //                     Email = r.Account.Email,
        //                     MainImageFileKey = r.Account.MainImageFileKey
        //                 },
        //                 Rating = r.Rating,
        //                 Content = r.Content,
        //                 DeletedAt = r.DeletedAt,
        //                 PodcastBuddyId = r.PodcastBuddyId,
        //                 Title = r.Title,
        //                 UpdatedAt = r.UpdatedAt
        //             }).ToList()
        //         };

        //     }
        //     catch (Exception ex)
        //     {
        //         Console.WriteLine("\n" + ex.StackTrace + "\n");
        //         throw new HttpRequestException("Get podcast buddy account failed, error: " + ex.Message);
        //     }
        // }

        // public async Task ChangeAccountStatus(ChangeAccountStatusParameterDTO changeAccountStatusParameter, SagaCommandMessage command)
        // {
        //     using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
        //     {
        //         try
        //         {
        //             var account = await _accountGenericRepository.FindByIdAsync(changeAccountStatusParameter.Id, includeProperties: a => a.PodcasterProfile);
        //             if (account == null)
        //             {
        //                 throw new Exception("Account with id " + changeAccountStatusParameter.Id + " does not exist");
        //             }

        //             var result = await _redisSharedCacheService.KeySetAsync<AccountStatusCache>($"account:status:{account.Id}", new AccountStatusCache
        //             {
        //                 Id = account.Id,
        //                 IsVerified = account.IsVerified,
        //                 DeactivatedAt = account.DeactivatedAt,
        //                 RoleId = account.RoleId,
        //                 LastViolationLevelChanged = account.LastViolationLevelChanged,
        //                 LastViolationPointChanged = account.LastViolationPointChanged,
        //                 ViolationLevel = account.ViolationLevel,
        //                 ViolationPoint = account.ViolationPoint,
        //                 HasVerifiedPodcasterProfile = account.RoleId == 1 && account.PodcasterProfile != null && account.PodcasterProfile.IsVerified == true ? true : false
        //             },
        //             // set cache expiry to 1 hour (khi hết hạn key-value này sẽ bị xoá khỏi redis)
        //             TimeSpan.FromHours(1)
        //             );

        //             if (result == false)
        //             {
        //                 throw new Exception("Change account status failed, cannot update account status cache in redis");
        //             }

        //             var messageNextRequestData = command.RequestData;
        //             messageNextRequestData["Id"] = account.Id;
        //             var messageResponseData = JObject.FromObject(new
        //             {
        //                 Id = account.Id,
        //             });

        //             var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
        //                 topic: KafkaTopicEnum.UserManagementDomain,
        //                 requestData: messageNextRequestData,
        //                 responseData: messageResponseData,
        //                 sagaInstanceId: command.SagaInstanceId,
        //                 flowName: command.FlowName,
        //                 messageName: "change-account-status.success"
        //                 );
        //             await _messagingService.SendSagaMessageAsync(sagaEventMessage);
        //         }
        //         catch (Exception ex)
        //         {
        //             await transaction.RollbackAsync();

        //             var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
        //                 topic: KafkaTopicEnum.UserManagementDomain,
        //                 requestData: command.RequestData,
        //                 responseData: JObject.FromObject(new
        //                 {
        //                     ErrorMessage = $"Change account status failed, error: {ex.Message}"
        //                 }),
        //                 sagaInstanceId: command.SagaInstanceId,
        //                 flowName: command.FlowName,
        //                 messageName: "change-account-status.failed"
        //                 );
        //             await _messagingService.SendSagaMessageAsync(sagaEventMessage);

        //             Console.WriteLine("\n" + ex.StackTrace + "\n");
        //         }
        //     }

        // }

        // public async Task CreatePodcasterProfile(CreatePodcasterProfileParameterDTO createPodcasterProfileParameter, SagaCommandMessage command)
        // {
        //     using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
        //     {
        //         try
        //         {
        //             var activeSystemConfigProfile = await GetActiveSystemConfigProfile();
        //             var account = await _accountGenericRepository.FindByIdAsync(createPodcasterProfileParameter.AccountId, includeProperties: a => a.PodcasterProfile);
        //             // Console.WriteLine("Tìm thấy account: " + (account.PodcasterProfile.IsVerified == null ? "null" : account.PodcasterProfile.IsVerified.ToString()));
        //             if (account.PodcasterProfile != null)
        //             {
        //                 if (account.PodcasterProfile.IsVerified == true)
        //                 {
        //                     throw new Exception("Podcaster profile for account id " + createPodcasterProfileParameter.AccountId + " is already verified, cannot create another one");
        //                 }
        //                 else if (account.PodcasterProfile.IsVerified == null)
        //                 {
        //                     throw new Exception("Podcaster profile for account id " + createPodcasterProfileParameter.AccountId + " is pending verification, cannot create another one");
        //                 }
        //                 else
        //                 {
        //                     // xoá profile cũ
        //                     Console.WriteLine("Xoá podcaster profile cũ cho account id: " + account.Id);
        //                     await _podcasterProfileGenericRepository.DeleteAsync(account.PodcasterProfile.AccountId);
        //                 }
        //             }



        //             var podcasterProfile = new PodcasterProfile
        //             {
        //                 AccountId = account.Id,
        //                 Name = createPodcasterProfileParameter.Name,
        //                 Description = createPodcasterProfileParameter.Description,
        //                 BuddyAudioFileKey = null,
        //                 CommitmentDocumentFileKey = null,
        //                 IsVerified = null,
        //                 OwnedBookingStorageSize = activeSystemConfigProfile["BookingConfig"].Value<double>("FreeInitialBookingStorageSize"),
        //                 UsedBookingStorageSize = 0,
        //                 RatingCount = 0
        //             };


        //             var folderPath = _filePathConfig.ACCOUNT_FILE_PATH + "\\" + account.Id;
        //             if (createPodcasterProfileParameter.CommitmentDocumentFileKey != null && createPodcasterProfileParameter.CommitmentDocumentFileKey != "")
        //             {
        //                 var CommitmentDocumentFileKey = FilePathHelper.CombinePaths(folderPath, $"buddy_commitment_document{FilePathHelper.GetExtension(createPodcasterProfileParameter.CommitmentDocumentFileKey)}");
        //                 await _fileIOHelper.CopyFileToFileAsync(createPodcasterProfileParameter.CommitmentDocumentFileKey, CommitmentDocumentFileKey);
        //                 await _fileIOHelper.DeleteFileAsync(createPodcasterProfileParameter.CommitmentDocumentFileKey);
        //                 podcasterProfile.CommitmentDocumentFileKey = CommitmentDocumentFileKey;

        //             }


        //             await _podcasterProfileGenericRepository.CreateAsync(podcasterProfile);
        //             await transaction.CommitAsync();

        //             var messageNextRequestData = JObject.FromObject(createPodcasterProfileParameter);
        //             messageNextRequestData["AccountId"] = podcasterProfile.AccountId;
        //             messageNextRequestData["Name"] = podcasterProfile.Name;
        //             messageNextRequestData["Description"] = podcasterProfile.Description;
        //             messageNextRequestData["CommitmentDocumentFileKey"] = podcasterProfile.CommitmentDocumentFileKey;

        //             var messageResponseData = JObject.FromObject(new
        //             {
        //                 // Message = "Create podcaster profile successfully",
        //                 AccountId = podcasterProfile.AccountId,
        //                 Name = podcasterProfile.Name,
        //                 Description = podcasterProfile.Description,
        //                 CommitmentDocumentFileKey = podcasterProfile.CommitmentDocumentFileKey
        //             });
        //             var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
        //                 topic: KafkaTopicEnum.UserManagementDomain,
        //                 requestData: messageNextRequestData,
        //                 responseData: messageResponseData,
        //                 sagaInstanceId: command.SagaInstanceId,
        //                 flowName: command.FlowName,
        //                 messageName: "create-podcaster-profile.success"
        //                 );
        //             await _messagingService.SendSagaMessageAsync(sagaEventMessage);
        //             await SendChangeAccountStatusMessage(podcasterProfile.AccountId);



        //         }
        //         catch (Exception ex)
        //         {
        //             await transaction.RollbackAsync();

        //             var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
        //                 topic: KafkaTopicEnum.UserManagementDomain,
        //                 requestData: command.RequestData,
        //                 responseData: JObject.FromObject(new
        //                 {
        //                     ErrorMessage = $"Create podcaster profile failed, error: {ex.Message}"
        //                 }),
        //                 sagaInstanceId: command.SagaInstanceId,
        //                 flowName: command.FlowName,
        //                 messageName: "create-podcaster-profile.failed"
        //                 );
        //             await _messagingService.SendSagaMessageAsync(sagaEventMessage);

        //             Console.WriteLine("\n" + ex.StackTrace + "\n");
        //         }
        //     }
        // }

        // public async Task<AccountListItemResponseDTO> GetAccountById(int accountId)
        // {
        //     try
        //     {
        //         var account = await _accountGenericRepository.FindByIdAsync(accountId, includeProperties: a => a.Role);

        //         return new AccountListItemResponseDTO
        //         {
        //             Id = account.Id,
        //             Email = account.Email,
        //             Role = new RoleDTO
        //             {
        //                 Id = account.Role.Id,
        //                 Name = account.Role.Name
        //             },
        //             FullName = account.FullName,
        //             Dob = account.Dob?.ToString("yyyy-MM-dd"),
        //             Gender = account.Gender,
        //             Address = account.Address,
        //             Phone = account.Phone,
        //             Balance = account.Balance,
        //             IsVerified = account.IsVerified,
        //             PodcastListenSlot = account.PodcastListenSlot,
        //             ViolationPoint = account.ViolationPoint,
        //             CreatedAt = account.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
        //             UpdatedAt = account.UpdatedAt.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
        //             MainImageFileKey = account.MainImageFileKey,
        //             DeactivatedAt = account.DeactivatedAt?.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
        //             ViolationLevel = account.ViolationLevel,
        //             LastViolationPointChanged = account.LastViolationPointChanged?.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
        //             LastViolationLevelChanged = account.LastViolationLevelChanged?.ToString("yyyy-MM-ddTH:mm:ss.fffZ"),
        //             LastPodcastListenSlotChanged = account.LastPodcastListenSlotChanged?.ToString("yyyy-MM-ddTH:mm:ss.fffZ"),
        //         };
        //     }
        //     catch (Exception ex)
        //     {
        //         Console.WriteLine("\n" + ex.StackTrace + "\n");
        //         throw new HttpRequestException("Get account by id failed, error: " + ex.Message);
        //     }


        // }


        // public async Task UpdatePodcasterProfile(UpdatePodcasterProfileParameterDTO updatePodcasterProfileParameterDTO, SagaCommandMessage command)
        // {
        //     using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
        //     {
        //         try
        //         {
        //             var podcasterProfile = (await _podcasterProfileGenericRepository.FindAll(
        //                 predicate: a => a.AccountId == updatePodcasterProfileParameterDTO.AccountId,
        //                 includeFunc: null
        //                 ).ToListAsync()).FirstOrDefault();


        //             if (podcasterProfile == null)
        //             {
        //                 throw new Exception("Podcaster profile with id " + updatePodcasterProfileParameterDTO.AccountId + " does not exist");
        //             }
        //             else if (podcasterProfile.IsVerified == false)
        //             {
        //                 throw new Exception("Podcaster profile with id " + updatePodcasterProfileParameterDTO.AccountId + " is not verified, cannot update");
        //             }




        //             podcasterProfile.Name = updatePodcasterProfileParameterDTO.Name;
        //             podcasterProfile.Description = updatePodcasterProfileParameterDTO.Description;

        //             var folderPath = _filePathConfig.ACCOUNT_FILE_PATH + "\\" + podcasterProfile.AccountId;
        //             if (updatePodcasterProfileParameterDTO.BuddyAudioFileKey != null && updatePodcasterProfileParameterDTO.BuddyAudioFileKey != "")
        //             {
        //                 await _fileIOHelper.DeleteFileAsync(podcasterProfile.BuddyAudioFileKey);
        //                 var BuddyAudioFileKey = FilePathHelper.CombinePaths(folderPath, $"buddy_trailer_audio{FilePathHelper.GetExtension(updatePodcasterProfileParameterDTO.BuddyAudioFileKey)}");
        //                 await _fileIOHelper.CopyFileToFileAsync(updatePodcasterProfileParameterDTO.BuddyAudioFileKey, BuddyAudioFileKey);
        //                 await _fileIOHelper.DeleteFileAsync(updatePodcasterProfileParameterDTO.BuddyAudioFileKey);
        //                 podcasterProfile.BuddyAudioFileKey = BuddyAudioFileKey;

        //             }

        //             await _podcasterProfileGenericRepository.UpdateAsync(podcasterProfile.AccountId, podcasterProfile);
        //             await transaction.CommitAsync();

        //             var messageNextRequestData = command.RequestData;
        //             messageNextRequestData["AccountId"] = podcasterProfile.AccountId;
        //             messageNextRequestData["Name"] = podcasterProfile.Name;
        //             messageNextRequestData["Description"] = podcasterProfile.Description;
        //             messageNextRequestData["BuddyAudioFileKey"] = podcasterProfile.BuddyAudioFileKey;
        //             var messageResponseData = JObject.FromObject(new
        //             {
        //                 // Message = "Update podcaster profile successfully",
        //                 AccountId = podcasterProfile.AccountId,
        //                 Name = podcasterProfile.Name,
        //                 Description = podcasterProfile.Description,
        //                 BuddyAudioFileKey = podcasterProfile.BuddyAudioFileKey
        //             });
        //             var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
        //                 topic: KafkaTopicEnum.UserManagementDomain,
        //                 requestData: messageNextRequestData,
        //                 responseData: messageResponseData,
        //                 sagaInstanceId: command.SagaInstanceId,
        //                 flowName: command.FlowName,
        //                 messageName: "update-podcaster-profile.success"
        //                 );
        //             await _messagingService.SendSagaMessageAsync(sagaEventMessage);
        //             await SendChangeAccountStatusMessage(podcasterProfile.AccountId);
        //         }
        //         catch (Exception ex)
        //         {
        //             await transaction.RollbackAsync();

        //             var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
        //                 topic: KafkaTopicEnum.UserManagementDomain,
        //                 requestData: command.RequestData,
        //                 responseData: JObject.FromObject(new
        //                 {
        //                     ErrorMessage = $"Update podcaster profile failed, error: {ex.Message}"
        //                 }),
        //                 sagaInstanceId: command.SagaInstanceId,
        //                 flowName: command.FlowName,
        //                 messageName: "update-podcaster-profile.failed"
        //                 );
        //             await _messagingService.SendSagaMessageAsync(sagaEventMessage);

        //             Console.WriteLine("\n" + ex.StackTrace + "\n");
        //         }
        //     }
        // }

        // public async Task UpdateUser(UpdateUserParameterDTO updateUserParameterDTO, SagaCommandMessage command)
        // {
        //     using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
        //     {
        //         try
        //         {
        //             var updateUserInfo = updateUserParameterDTO;
        //             var account = await _accountGenericRepository.FindByIdAsync(updateUserInfo.AccountId);
        //             if (account == null)
        //             {
        //                 throw new Exception("Account with id " + updateUserInfo.AccountId + " does not exist");
        //             }
        //             account.FullName = updateUserInfo.FullName;
        //             account.Dob = DateOnly.FromDateTime(updateUserInfo.Dob);
        //             account.Gender = updateUserInfo.Gender;
        //             account.Address = updateUserInfo.Address;
        //             account.Phone = updateUserInfo.Phone;

        //             await _accountGenericRepository.UpdateAsync(account.Id, account);

        //             var folderPath = _filePathConfig.ACCOUNT_FILE_PATH + "\\" + account.Id;
        //             if (updateUserInfo.MainImageFileKey != null && updateUserInfo.MainImageFileKey != "")
        //             {
        //                 await _fileIOHelper.DeleteFileAsync(account.MainImageFileKey);
        //                 var MainImageFileKey = FilePathHelper.CombinePaths(folderPath, $"main_image{FilePathHelper.GetExtension(updateUserInfo.MainImageFileKey)}");
        //                 await _fileIOHelper.CopyFileToFileAsync(updateUserInfo.MainImageFileKey, MainImageFileKey);
        //                 await _fileIOHelper.DeleteFileAsync(updateUserInfo.MainImageFileKey);
        //                 account.MainImageFileKey = MainImageFileKey;
        //                 await _accountGenericRepository.UpdateAsync(account.Id, account);
        //             }

        //             await transaction.CommitAsync();

        //             var messageNextRequestData = command.RequestData;
        //             messageNextRequestData["AccountId"] = account.Id;
        //             messageNextRequestData["Email"] = account.Email;
        //             messageNextRequestData["FullName"] = account.FullName;
        //             messageNextRequestData["Dob"] = account.Dob?.ToString("yyyy-MM-dd");
        //             messageNextRequestData["Gender"] = account.Gender;
        //             messageNextRequestData["Address"] = account.Address;
        //             messageNextRequestData["Phone"] = account.Phone;
        //             messageNextRequestData["MainImageFileKey"] = account.MainImageFileKey;

        //             var messageResponseData = JObject.FromObject(new
        //             {
        //                 // Message = "Update user successfully",
        //                 AccountId = account.Id,
        //                 Email = account.Email,
        //                 FullName = account.FullName,
        //                 Dob = account.Dob?.ToString("yyyy-MM-dd"),
        //                 Gender = account.Gender,
        //                 Address = account.Address,
        //                 Phone = account.Phone,
        //                 MainImageFileKey = account.MainImageFileKey
        //             });
        //             var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
        //                 topic: KafkaTopicEnum.UserManagementDomain,
        //                 requestData: messageNextRequestData,
        //                 responseData: messageResponseData,
        //                 sagaInstanceId: command.SagaInstanceId,
        //                 flowName: command.FlowName,
        //                 messageName: "update-user.success"
        //             );
        //             await _messagingService.SendSagaMessageAsync(sagaEventMessage);
        //             await SendChangeAccountStatusMessage(account.Id);

        //         }
        //         catch (Exception ex)
        //         {
        //             await transaction.RollbackAsync();

        //             var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
        //                 topic: KafkaTopicEnum.UserManagementDomain,
        //                 requestData: command.RequestData,
        //                 responseData: JObject.FromObject(new
        //                 {
        //                     ErrorMessage = $"Update user failed, error: {ex.Message}"
        //                 }),
        //                 sagaInstanceId: command.SagaInstanceId,
        //                 flowName: command.FlowName,
        //                 messageName: "update-user.failed"
        //             );
        //             await _messagingService.SendSagaMessageAsync(sagaEventMessage);

        //             Console.WriteLine("\n" + ex.StackTrace + "\n");
        //         }
        //     }
        // }

        // public async Task DeactivateAccount(DeactivateAccountParameterDTO deactivateAccountParameterDTO, SagaCommandMessage command)
        // {
        //     using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
        //     {
        //         try
        //         {
        //             var account = await this.GetExistAccountById(deactivateAccountParameterDTO.AccountId);
        //             account.DeactivatedAt = _dateHelper.GetNowByAppTimeZone();

        //             await _accountGenericRepository.UpdateAsync(account.Id, account);

        //             await transaction.CommitAsync();

        //             var messageNextRequestData = command.RequestData;
        //             messageNextRequestData["AccountId"] = account.Id;
        //             var messageResponseData = JObject.FromObject(new
        //             {
        //                 // Message = "Deactivate account successfully"
        //                 AccountId = account.Id,
        //             });
        //             var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
        //                 topic: KafkaTopicEnum.UserManagementDomain,
        //                 requestData: messageNextRequestData,
        //                 responseData: messageResponseData,
        //                 sagaInstanceId: command.SagaInstanceId,
        //                 flowName: command.FlowName,
        //                 messageName: "deactivate-account.success"
        //             );
        //             await _messagingService.SendSagaMessageAsync(sagaEventMessage);
        //             await SendChangeAccountStatusMessage(account.Id);
        //             // [CHỈNH SỬA SAU] CHẠY CÁC FLOW XOÁ TRONG booking, chanel/show/episode (AccountFavoritedPodcastChannel/AccountFollowedPodcastShow/AccountSavedPodcastEpisode), podcast subscription, Report review session, publish review session, DMCA Accusation, AccountFollowedPodcaster

        //         }
        //         catch (Exception ex)
        //         {
        //             await transaction.RollbackAsync();

        //             var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
        //                 topic: KafkaTopicEnum.UserManagementDomain,
        //                 requestData: command.RequestData,
        //                 responseData: JObject.FromObject(new
        //                 {
        //                     ErrorMessage = $"Deactivate account failed, error: {ex.Message}"
        //                 }),
        //                 sagaInstanceId: command.SagaInstanceId,
        //                 flowName: command.FlowName,
        //                 messageName: "deactivate-account.failed"
        //             );
        //             await _messagingService.SendSagaMessageAsync(sagaEventMessage);

        //             Console.WriteLine("\n" + ex.StackTrace + "\n");
        //         }
        //     }
        // }
        #endregion

        public async Task<List<ChannelListItemResponseDTO>> GetChannels(int? roleId)
        {
            // tuân thủ cách viết ở trên
            try
            {
                var query = _podcastChannelGenericRepository.FindAll(
                    predicate: c => c.DeletedAt == null,
                    includeFunc: q => q
                        .Include(pc => pc.PodcastCategory)
                        .Include(pc => pc.PodcastSubCategory)
                        .Include(pc => pc.PodcastChannelStatusTrackings)
                        .ThenInclude(pct => pct.PodcastChannelStatus)
                        .Include(pc => pc.PodcastChannelHashtags)
                        .ThenInclude(pch => pch.Hashtag)
                        .Include(pc => pc.PodcastShows)
                        .ThenInclude(ps => ps.PodcastShowStatusTrackings)
                );

                if (roleId == null || roleId == 1)
                {
                    query = query.Where(pc => pc.PodcastChannelStatusTrackings.OrderByDescending(pct => pct.CreatedAt).FirstOrDefault().PodcastChannelStatusId == (int)PodcastChannelStatusEnum.Published);
                }
                var channels = await query.ToListAsync();

                var channelList = (await Task.WhenAll(channels.Select(async pc =>
                {
                    var podcaster = await _accountCachingService.GetAccountStatusCacheById(pc.PodcasterId);
                    if (podcaster == null || podcaster.Id != pc.PodcasterId || podcaster.IsVerified == false || podcaster.HasVerifiedPodcasterProfile == false)
                    {
                        throw new Exception("Podcaster with id " + pc.PodcasterId + " does not exist");
                    }
                    return new ChannelListItemResponseDTO
                    {
                        Id = pc.Id,
                        Name = pc.Name,
                        Description = pc.Description,
                        MainImageFileKey = pc.MainImageFileKey,
                        BackgroundImageFileKey = pc.BackgroundImageFileKey,
                        PodcastCategory = new PodcastCategoryDTO
                        {
                            Id = pc.PodcastCategory.Id,
                            Name = pc.PodcastCategory.Name
                        },
                        PodcastSubCategory = new PodcastSubCategoryDTO
                        {
                            Id = pc.PodcastSubCategory.Id,
                            Name = pc.PodcastSubCategory.Name,
                            PodcastCategoryId = pc.PodcastSubCategory.PodcastCategoryId
                        },
                        CurrentStatus = pc.PodcastChannelStatusTrackings.OrderByDescending(pct => pct.CreatedAt).Select(pct => new PodcastChannelStatusDTO
                        {
                            Id = pct.PodcastChannelStatus.Id,
                            Name = pct.PodcastChannelStatus.Name
                        }).FirstOrDefault()!,
                        Hashtags = pc.PodcastChannelHashtags.Select(pch => new HashtagDTO
                        {
                            Id = pch.Hashtag.Id,
                            Name = pch.Hashtag.Name
                        }).ToList(),
                        TotalFavorite = pc.TotalFavorite,
                        ListenCount = pc.ListenCount,
                        ShowCount = pc.PodcastShows != null ? pc.PodcastShows.Count(ps => ps.PodcastShowStatusTrackings.OrderByDescending(pst => pst.CreatedAt).FirstOrDefault().PodcastShowStatusId != (int)PodcastShowStatusEnums.Published && ps.DeletedAt == null) : 0,
                        Podcaster = new AccountSnippetResponseDTO
                        {
                            Id = podcaster.Id,
                            FullName = podcaster.FullName,
                            Email = podcaster.Email,
                            MainImageFileKey = podcaster.MainImageFileKey
                        },
                        CreatedAt = pc.CreatedAt,
                        UpdatedAt = pc.UpdatedAt
                    };
                }))).ToList();
                return channelList;
            }
            catch (Exception ex)
            {
                Console.WriteLine("\n" + ex.StackTrace + "\n");
                throw new HttpRequestException("Get channels failed, error: " + ex.Message);
            }
        }

        public async Task<List<ChannelListItemResponseDTO>> GetChannelByPodcasterIdAsync(int podcasterId)
        {
            // tuân thủ cách viết ở trên
            try
            {
                var query = _podcastChannelGenericRepository.FindAll(
                    predicate: c => c.DeletedAt == null && c.PodcasterId == podcasterId,
                    includeFunc: q => q
                        .Include(pc => pc.PodcastCategory)
                        .Include(pc => pc.PodcastSubCategory)
                        .Include(pc => pc.PodcastChannelStatusTrackings)
                        .ThenInclude(pct => pct.PodcastChannelStatus)
                        .Include(pc => pc.PodcastChannelHashtags)
                        .ThenInclude(pch => pch.Hashtag)
                        .Include(pc => pc.PodcastShows)
                        .ThenInclude(ps => ps.PodcastShowStatusTrackings)
                );

                var channels = await query.ToListAsync();

                var channelList = (await Task.WhenAll(channels.Select(async pc =>
                {
                    var podcaster = await _accountCachingService.GetAccountStatusCacheById(pc.PodcasterId);
                    if (podcaster == null || podcaster.Id != pc.PodcasterId || podcaster.IsVerified == false || podcaster.HasVerifiedPodcasterProfile == false)
                    {
                        throw new Exception("Podcaster with id " + pc.PodcasterId + " does not exist");
                    }
                    return new ChannelListItemResponseDTO
                    {
                        Id = pc.Id,
                        Name = pc.Name,
                        Description = pc.Description,
                        MainImageFileKey = pc.MainImageFileKey,
                        BackgroundImageFileKey = pc.BackgroundImageFileKey,
                        PodcastCategory = new PodcastCategoryDTO
                        {
                            Id = pc.PodcastCategory.Id,
                            Name = pc.PodcastCategory.Name
                        },
                        PodcastSubCategory = new PodcastSubCategoryDTO
                        {
                            Id = pc.PodcastSubCategory.Id,
                            Name = pc.PodcastSubCategory.Name,
                            PodcastCategoryId = pc.PodcastSubCategory.PodcastCategoryId
                        },
                        CurrentStatus = pc.PodcastChannelStatusTrackings.OrderByDescending(pct => pct.CreatedAt).Select(pct => new PodcastChannelStatusDTO
                        {
                            Id = pct.PodcastChannelStatus.Id,
                            Name = pct.PodcastChannelStatus.Name
                        }).FirstOrDefault()!,
                        Hashtags = pc.PodcastChannelHashtags.Select(pch => new HashtagDTO
                        {
                            Id = pch.Hashtag.Id,
                            Name = pch.Hashtag.Name
                        }).ToList(),
                        TotalFavorite = pc.TotalFavorite,
                        ListenCount = pc.ListenCount,
                        ShowCount = pc.PodcastShows != null ? pc.PodcastShows.Count(ps => ps.DeletedAt == null) : 0,
                        Podcaster = new AccountSnippetResponseDTO
                        {
                            Id = podcaster.Id,
                            FullName = podcaster.FullName,
                            Email = podcaster.Email,
                            MainImageFileKey = podcaster.MainImageFileKey
                        },
                        CreatedAt = pc.CreatedAt,
                        UpdatedAt = pc.UpdatedAt
                    };
                }))

                ).ToList();
                return channelList;
            }
            catch (Exception ex)
            {
                Console.WriteLine("\n" + ex.StackTrace + "\n");
                throw new HttpRequestException("Get channels failed, error: " + ex.Message);
            }
        }

        public async Task<ChannelDetailResponseDTO> GetChannelByIdAsync(Guid channelId, int? role)
        {
            try
            {
                var query = _podcastChannelGenericRepository.FindAll(
                    predicate: c => c.DeletedAt == null && c.Id == channelId,
                    includeFunc: q => q
                        .Include(pc => pc.PodcastCategory)
                        .Include(pc => pc.PodcastSubCategory)
                        .Include(pc => pc.PodcastChannelStatusTrackings)
                        .ThenInclude(pct => pct.PodcastChannelStatus)
                        .Include(pc => pc.PodcastChannelHashtags)
                        .ThenInclude(pch => pch.Hashtag)
                        .Include(pc => pc.PodcastShows)
                        .ThenInclude(ps => ps.PodcastShowStatusTrackings)
                );

                if (role == null || role == 1)
                {
                    query = query.Where(pc => pc.PodcastChannelStatusTrackings.OrderByDescending(pct => pct.CreatedAt).FirstOrDefault().PodcastChannelStatusId == (int)PodcastChannelStatusEnum.Published);
                }

                var channel = await query.FirstOrDefaultAsync();

                if (channel == null)
                {
                    throw new Exception("Channel with id " + channelId + " does not exist");
                }

                var podcastSubscriptionBatchRequest = new BatchQueryRequest
                {
                    Queries = new List<BatchQueryItem>
                    {
                        new BatchQueryItem
                        {
                            Key = "podcastSubscriptionList",
                            QueryType = "findall",
                            EntityType = "PodcastSubscription",
                            Parameters = JObject.FromObject(new
                            {
                                where = (role == null || role == 1) ? new
                                {
                                    IsActive = (bool?)true,
                                    DeletedAt = (DateTime?)null,
                                    PodcastChannelId = channel.Id,
                                } : new {
                                    IsActive = (bool?)null,
                                    DeletedAt = (DateTime?)null,
                                    PodcastChannelId = channel.Id,
                                },
                                include = "PodcastSubscriptionBenefitMappings.PodcastSubscriptionBenefit , PodcastSubscriptionCycleTypePrices.SubscriptionCycleType"
                            }),
                        }
                    }
                };

                var result = await _httpServiceQueryClient.ExecuteBatchAsync("SubscriptionService", podcastSubscriptionBatchRequest);


                var showByChannelIdQuery = _podcastShowGenericRepository.FindAll(
                    predicate: ps => ps.DeletedAt == null && ps.PodcastChannelId == channel.Id,
                    includeFunc: q => q
                        .Include(ps => ps.PodcastShowStatusTrackings)
                        .ThenInclude(pst => pst.PodcastShowStatus)
                        .Include(ps => ps.PodcastCategory)
                        .Include(ps => ps.PodcastSubCategory)
                        .Include(ps => ps.PodcastShowHashtags)
                        .ThenInclude(psh => psh.Hashtag)
                        .Include(ps => ps.PodcastShowSubscriptionType)
                );

                if (role == null || role == 1)
                {
                    showByChannelIdQuery = showByChannelIdQuery.Where(ps => ps.PodcastShowStatusTrackings.OrderByDescending(pst => pst.CreatedAt).FirstOrDefault().PodcastShowStatusId == (int)PodcastShowStatusEnums.Published && ps.IsReleased == true);
                }
                var showList = await showByChannelIdQuery.ToListAsync();

                var podcaster = await _accountCachingService.GetAccountStatusCacheById(channel.PodcasterId);
                if (podcaster == null || podcaster.Id != channel.PodcasterId || podcaster.IsVerified == false || podcaster.HasVerifiedPodcasterProfile == false)
                {
                    throw new Exception("Podcaster with id " + channel.PodcasterId + " does not exist");
                }

                var channelDetail = new ChannelDetailResponseDTO
                {
                    Id = channel.Id,
                    Name = channel.Name,
                    Description = channel.Description,
                    MainImageFileKey = channel.MainImageFileKey,
                    BackgroundImageFileKey = channel.BackgroundImageFileKey,
                    PodcastCategory = new PodcastCategoryDTO
                    {
                        Id = channel.PodcastCategory.Id,
                        Name = channel.PodcastCategory.Name
                    },
                    PodcastSubCategory = new PodcastSubCategoryDTO
                    {
                        Id = channel.PodcastSubCategory.Id,
                        Name = channel.PodcastSubCategory.Name,
                        PodcastCategoryId = channel.PodcastSubCategory.PodcastCategoryId
                    },
                    CurrentStatus = channel.PodcastChannelStatusTrackings.OrderByDescending(pct => pct.CreatedAt).Select(pct => new PodcastChannelStatusDTO
                    {
                        Id = pct.PodcastChannelStatus.Id,
                        Name = pct.PodcastChannelStatus.Name
                    }).FirstOrDefault()!,
                    Hashtags = channel.PodcastChannelHashtags.Select(pch => new HashtagDTO
                    {
                        Id = pch.Hashtag.Id,
                        Name = pch.Hashtag.Name
                    }).ToList(),
                    TotalFavorite = channel.TotalFavorite,
                    ListenCount = channel.ListenCount,
                    ShowCount = showList.Count,
                    Podcaster = new AccountSnippetResponseDTO
                    {
                        Id = podcaster.Id,
                        FullName = podcaster.FullName,
                        Email = podcaster.Email,
                        MainImageFileKey = podcaster.MainImageFileKey
                    },
                    CreatedAt = channel.CreatedAt,
                    UpdatedAt = channel.UpdatedAt,
                    ShowList = showList.Select(ps => new ShowListItemResponseDTO
                    {
                        Id = ps.Id,
                        Name = ps.Name,
                        Description = ps.Description,
                        MainImageFileKey = ps.MainImageFileKey,
                        TrailerAudioFileKey = ps.TrailerAudioFileKey,
                        TotalFollow = ps.TotalFollow,
                        ListenCount = ps.ListenCount,
                        AverageRating = ps.AverageRating,
                        RatingCount = ps.RatingCount,
                        Copyright = ps.Copyright,
                        IsReleased = ps.IsReleased,
                        Language = ps.Language,
                        UploadFrequency = ps.UploadFrequency,
                        ReleaseDate = ps.ReleaseDate,
                        TakenDownReason = role == null || role == 1 ? null : ps.TakenDownReason,
                        PodcastCategory = new PodcastCategoryDTO
                        {
                            Id = ps.PodcastCategory.Id,
                            Name = ps.PodcastCategory.Name
                        },
                        PodcastSubCategory = new PodcastSubCategoryDTO
                        {
                            Id = ps.PodcastSubCategory.Id,
                            Name = ps.PodcastSubCategory.Name,
                            PodcastCategoryId = ps.PodcastSubCategory.PodcastCategoryId
                        },
                        PodcastChannel = new PodcastChannelSnippetResponseDTO
                        {
                            Id = channel.Id,
                            Name = channel.Name,
                            MainImageFileKey = channel.MainImageFileKey
                        },
                        Podcaster = new AccountSnippetResponseDTO
                        {
                            Id = podcaster.Id,
                            Email = podcaster.Email,
                            FullName = podcaster.FullName,
                            MainImageFileKey = podcaster.MainImageFileKey
                        },
                        PodcastShowSubscriptionType = ps.PodcastShowSubscriptionType != null ? new PodcastShowSubscriptionTypeDTO
                        {
                            Id = ps.PodcastShowSubscriptionType.Id,
                            Name = ps.PodcastShowSubscriptionType.Name
                        } : null,
                        Hashtags = ps.PodcastShowHashtags.Select(psh => new HashtagDTO
                        {
                            Id = psh.Hashtag.Id,
                            Name = psh.Hashtag.Name
                        }).ToList(),
                        CreatedAt = ps.CreatedAt,
                        UpdatedAt = ps.UpdatedAt,
                        CurrentStatus = ps.PodcastShowStatusTrackings.OrderByDescending(pst => pst.CreatedAt).Select(pst => new PodcastShowStatusDTO
                        {
                            Id = pst.PodcastShowStatus.Id,
                            Name = pst.PodcastShowStatus.Name
                        }).FirstOrDefault()!,
                    }).ToList(),
                    PodcastSubscriptionList = ((JArray)result.Results["podcastSubscriptionList"]).Select(ps =>
                    {
                        var psObj = ps.ToObject<PodcastSubscriptionListItemResponseDTO>();
                        return new PodcastSubscriptionListItemResponseDTO
                        {
                            Id = psObj.Id,
                            Name = psObj.Name,
                            Description = psObj.Description,
                            CurrentVersion = psObj.CurrentVersion,
                            PodcastShowId = psObj.PodcastShowId,
                            IsActive = psObj.IsActive,
                            CreatedAt = psObj.CreatedAt,
                            UpdatedAt = psObj.UpdatedAt,
                            PodcastChannelId = psObj.PodcastChannelId,
                            PodcastSubscriptionCycleTypePriceList = ((JArray)ps["PodcastSubscriptionCycleTypePrices"]).ToObject<List<PodcastSubscriptionCycleTypePriceListItemResponseDTO>>().Select(psctp => new PodcastSubscriptionCycleTypePriceListItemResponseDTO
                            {
                                PodcastSubscriptionId = psctp.PodcastSubscriptionId,
                                Price = psctp.Price,
                                Version = psctp.Version,
                                CreatedAt = psctp.CreatedAt,
                                UpdatedAt = psctp.UpdatedAt,
                                SubscriptionCycleType = psctp.SubscriptionCycleType != null ? new SubscriptionCycleTypeDTO
                                {
                                    Id = psctp.SubscriptionCycleType.Id,
                                    Name = psctp.SubscriptionCycleType.Name,
                                } : null
                            }).ToList(),
                            DeletedAt = psObj.DeletedAt,
                            PodcastSubscriptionBenefitMappingList = ((JArray)ps["PodcastSubscriptionBenefitMappings"]).ToObject<List<PodcastSubscriptionBenefitMappingListItemResponseDTO>>().Select(psbm => new PodcastSubscriptionBenefitMappingListItemResponseDTO
                            {
                                PodcastSubscriptionId = psbm.PodcastSubscriptionId,
                                Version = psbm.Version,
                                CreatedAt = psbm.CreatedAt,
                                UpdatedAt = psbm.UpdatedAt,
                                PodcastSubscriptionBenefit = psbm.PodcastSubscriptionBenefit != null ? new PodcastSubscriptionBenefitDTO
                                {
                                    Id = psbm.PodcastSubscriptionBenefit.Id,
                                    Name = psbm.PodcastSubscriptionBenefit.Name
                                } : null
                            }).ToList()
                        };
                    }).ToList()

                };
                return channelDetail;
            }
            catch (Exception ex)
            {
                Console.WriteLine("\n" + ex.StackTrace + "\n");
                throw new HttpRequestException("Get channel by id failed, error: " + ex.Message);
            }
        }


        public async Task CreatePodcastChannel(CreateChannelParameterDTO createChannelParameterDTO, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var podcastChannel = new PodcastChannel
                    {
                        Name = createChannelParameterDTO.Name,
                        Description = createChannelParameterDTO.Description,
                        PodcasterId = createChannelParameterDTO.PodcasterId,
                        PodcastCategoryId = createChannelParameterDTO.PodcastCategoryId,
                        PodcastSubCategoryId = createChannelParameterDTO.PodcastSubCategoryId,
                    };

                    await _podcastChannelGenericRepository.CreateAsync(podcastChannel);

                    var newPodcastChannelStatusTracking = new PodcastChannelStatusTracking
                    {
                        PodcastChannelId = podcastChannel.Id,
                        PodcastChannelStatusId = (int)PodcastChannelStatusEnum.Unpublished, // setting to "Unpublished" status
                    };
                    await _podcastChannelStatusTrackingGenericRepository.CreateAsync(newPodcastChannelStatusTracking);


                    var folderPath = _filePathConfig.PODCAST_CHANNEL_FILE_PATH + "\\" + podcastChannel.Id;
                    if (createChannelParameterDTO.MainImageFileKey != null && createChannelParameterDTO.MainImageFileKey != "")
                    {
                        var MainImageFileKey = FilePathHelper.CombinePaths(folderPath, $"main_image{FilePathHelper.GetExtension(createChannelParameterDTO.MainImageFileKey)}");
                        await _fileIOHelper.CopyFileToFileAsync(createChannelParameterDTO.MainImageFileKey, MainImageFileKey);
                        await _fileIOHelper.DeleteFileAsync(createChannelParameterDTO.MainImageFileKey);
                        podcastChannel.MainImageFileKey = MainImageFileKey;

                    }
                    if (createChannelParameterDTO.BackgroundImageFileKey != null && createChannelParameterDTO.BackgroundImageFileKey != "")
                    {
                        var BackgroundImageFileKey = FilePathHelper.CombinePaths(folderPath, $"background_image{FilePathHelper.GetExtension(createChannelParameterDTO.BackgroundImageFileKey)}");
                        await _fileIOHelper.CopyFileToFileAsync(createChannelParameterDTO.BackgroundImageFileKey, BackgroundImageFileKey);
                        await _fileIOHelper.DeleteFileAsync(createChannelParameterDTO.BackgroundImageFileKey);
                        podcastChannel.BackgroundImageFileKey = BackgroundImageFileKey;
                    }

                    await _podcastChannelGenericRepository.UpdateAsync(podcastChannel.Id, podcastChannel);

                    foreach (var hashtagId in createChannelParameterDTO.HashtagIds)
                    {
                        var existingHashtag = await _hashtagGenericRepository.FindByIdAsync(hashtagId);
                        if (existingHashtag == null)
                        {
                            throw new Exception("Hashtag with id " + hashtagId + " does not exist");
                        }
                        var podcastChannelHashtag = new PodcastChannelHashtag
                        {
                            PodcastChannelId = podcastChannel.Id,
                            HashtagId = hashtagId
                        };
                        await _podcastChannelHashtagGenericRepository.CreateAsync(podcastChannelHashtag);
                    }

                    await transaction.CommitAsync();

                    var messageNextRequestData = command.RequestData;
                    messageNextRequestData["Name"] = podcastChannel.Name;
                    messageNextRequestData["Description"] = podcastChannel.Description;
                    messageNextRequestData["MainImageFileKey"] = podcastChannel.MainImageFileKey;
                    messageNextRequestData["BackgroundImageFileKey"] = podcastChannel.BackgroundImageFileKey;
                    messageNextRequestData["PodcastCategoryId"] = podcastChannel.PodcastCategoryId;
                    messageNextRequestData["PodcastSubCategoryId"] = podcastChannel.PodcastSubCategoryId;
                    messageNextRequestData["HashtagIds"] = JArray.FromObject(createChannelParameterDTO.HashtagIds);
                    messageNextRequestData["PodcasterId"] = podcastChannel.PodcasterId;

                    var messageResponseData = JObject.FromObject(new
                    {
                        PodcastChannelId = podcastChannel.Id,
                        Name = podcastChannel.Name,
                        Description = podcastChannel.Description,
                        MainImageFileKey = podcastChannel.MainImageFileKey,
                        BackgroundImageFileKey = podcastChannel.BackgroundImageFileKey,
                        PodcastCategoryId = podcastChannel.PodcastCategoryId,
                        PodcastSubCategoryId = podcastChannel.PodcastSubCategoryId,
                        HashtagIds = createChannelParameterDTO.HashtagIds,
                        PodcasterId = podcastChannel.PodcasterId
                    });
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.ContentManagementDomain,
                        requestData: messageNextRequestData,
                        responseData: messageResponseData,
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "create-channel.success"
                    );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);

                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();

                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.ContentManagementDomain,
                        requestData: command.RequestData,
                        responseData: JObject.FromObject(new
                        {
                            ErrorMessage = $"Create podcast channel failed, error: {ex.Message}"
                        }),
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "create-channel.failed"
                    );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);

                    Console.WriteLine("\n" + ex.StackTrace + "\n");
                }
            }
        }


        public async Task UpdatePodcastChannel(UpdateChannelParameterDTO updateChannelParameterDTO, SagaCommandMessage command)
        {
                using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
                {
                    try
                    {
                    var podcastChannel = await _podcastChannelGenericRepository.FindByIdAsync(updateChannelParameterDTO.PodcastChannelId);
                    if (podcastChannel == null)
                    {
                        throw new Exception("Podcast channel with id " + updateChannelParameterDTO.PodcastChannelId + " does not exist");
                    }
                    else if (podcastChannel.DeletedAt != null)
                    {
                        throw new Exception("Podcast channel with id " + updateChannelParameterDTO.PodcastChannelId + " has been deleted");
                    }
                    else if (podcastChannel.PodcasterId != updateChannelParameterDTO.PodcasterId)
                    {
                        throw new Exception("Podcast channel with id " + updateChannelParameterDTO.PodcastChannelId + " does not belong to podcaster with id " + updateChannelParameterDTO.PodcasterId);
                    }

                    podcastChannel.Name = updateChannelParameterDTO.Name;
                    podcastChannel.Description = updateChannelParameterDTO.Description;
                    podcastChannel.PodcastCategoryId = updateChannelParameterDTO.PodcastCategoryId;
                    podcastChannel.PodcastSubCategoryId = updateChannelParameterDTO.PodcastSubCategoryId;

                    var folderPath = _filePathConfig.PODCAST_CHANNEL_FILE_PATH + "\\" + podcastChannel.Id;
                    if (updateChannelParameterDTO.MainImageFileKey != null && updateChannelParameterDTO.MainImageFileKey != "")
                    {
                        if (!string.IsNullOrEmpty(podcastChannel?.MainImageFileKey))
                        {
                            await _fileIOHelper.DeleteFileAsync(podcastChannel.MainImageFileKey);
                        }
                        var MainImageFileKey = FilePathHelper.CombinePaths(folderPath, $"main_image{FilePathHelper.GetExtension(updateChannelParameterDTO.MainImageFileKey)}");
                        await _fileIOHelper.CopyFileToFileAsync(updateChannelParameterDTO.MainImageFileKey, MainImageFileKey);
                        await _fileIOHelper.DeleteFileAsync(updateChannelParameterDTO.MainImageFileKey);
                        podcastChannel.MainImageFileKey = MainImageFileKey;

                    }
                    if (updateChannelParameterDTO.BackgroundImageFileKey != null && updateChannelParameterDTO.BackgroundImageFileKey != "")
                    {
                        if (!string.IsNullOrEmpty(podcastChannel?.BackgroundImageFileKey))
                        {
                            await _fileIOHelper.DeleteFileAsync(podcastChannel.BackgroundImageFileKey);
                        }
                        var BackgroundImageFileKey = FilePathHelper.CombinePaths(folderPath, $"background_image{FilePathHelper.GetExtension(updateChannelParameterDTO.BackgroundImageFileKey)}");
                        await _fileIOHelper.CopyFileToFileAsync(updateChannelParameterDTO.BackgroundImageFileKey, BackgroundImageFileKey);
                        await _fileIOHelper.DeleteFileAsync(updateChannelParameterDTO.BackgroundImageFileKey);
                        podcastChannel.BackgroundImageFileKey = BackgroundImageFileKey;
                    }

                    await _podcastChannelGenericRepository.UpdateAsync(podcastChannel.Id, podcastChannel);

                    // Update hashtags
                    await _unitOfWork.PodcastChannelHashtagRepository.DeleteByPodcastChannelIdAsync(podcastChannel.Id);

                    foreach (var hashtagId in updateChannelParameterDTO.HashtagIds)
                    {
                        var existingHashtag = await _hashtagGenericRepository.FindByIdAsync(hashtagId);
                        if (existingHashtag == null)
                        {
                            throw new Exception("Hashtag with id " + hashtagId + " does not exist");
                        }
                        var podcastChannelHashtag = new PodcastChannelHashtag
                        {
                            PodcastChannelId = podcastChannel.Id,
                            HashtagId = hashtagId
                        };
                        await _podcastChannelHashtagGenericRepository.CreateAsync(podcastChannelHashtag);
                    }
                    await transaction.CommitAsync();
                    var messageNextRequestData = command.RequestData;
                    messageNextRequestData["Name"] = podcastChannel.Name;
                    messageNextRequestData["Description"] = podcastChannel.Description;
                    messageNextRequestData["MainImageFileKey"] = podcastChannel.MainImageFileKey;
                    messageNextRequestData["BackgroundImageFileKey"] = podcastChannel.BackgroundImageFileKey;
                    messageNextRequestData["PodcastCategoryId"] = podcastChannel.PodcastCategoryId;
                    messageNextRequestData["PodcastSubCategoryId"] = podcastChannel.PodcastSubCategoryId;
                    messageNextRequestData["HashtagIds"] = JArray.FromObject(updateChannelParameterDTO.HashtagIds);
                    messageNextRequestData["PodcasterId"] = podcastChannel.PodcasterId;
                    messageNextRequestData["PodcastChannelId"] = podcastChannel.Id;

                    var messageResponseData = JObject.FromObject(new
                    {
                        PodcastChannelId = podcastChannel.Id,
                        Name = podcastChannel.Name,
                        Description = podcastChannel.Description,
                        MainImageFileKey = podcastChannel.MainImageFileKey,
                        BackgroundImageFileKey = podcastChannel.BackgroundImageFileKey,
                        PodcastCategoryId = podcastChannel.PodcastCategoryId,
                        PodcastSubCategoryId = podcastChannel.PodcastSubCategoryId,
                        HashtagIds = updateChannelParameterDTO.HashtagIds,
                        PodcasterId = podcastChannel.PodcasterId,
                    });
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.ContentManagementDomain,
                        requestData: messageNextRequestData,
                        responseData: messageResponseData,
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "update-channel.success"
                    );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();

                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.ContentManagementDomain,
                        requestData: command.RequestData,
                        responseData: JObject.FromObject(new
                        {
                            ErrorMessage = $"Update podcast channel failed, error: {ex.Message}"
                        }),
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "update-channel.failed"
                    );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);

                    Console.WriteLine("\n" + ex.StackTrace + "\n");
                }
            }
        }

        public async Task PublishPodcastChannel(PublishChannelParameterDTO publishChannelParameterDTO, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var podcastChannel = await _podcastChannelGenericRepository.FindByIdAsync(publishChannelParameterDTO.PodcastChannelId);
                    if (podcastChannel == null)
                    {
                        throw new Exception("Podcast channel with id " + publishChannelParameterDTO.PodcastChannelId + " does not exist");
                    }
                    else if (podcastChannel.DeletedAt != null)
                    {
                        throw new Exception("Podcast channel with id " + publishChannelParameterDTO.PodcastChannelId + " has been deleted");
                    }
                    else if (podcastChannel.PodcasterId != publishChannelParameterDTO.PodcasterId)
                    {
                        throw new Exception("Podcast channel with id " + publishChannelParameterDTO.PodcastChannelId + " does not belong to podcaster with id " + publishChannelParameterDTO.PodcasterId);
                    }

                    var newPodcastChannelStatusTracking = new PodcastChannelStatusTracking
                    {
                        PodcastChannelId = podcastChannel.Id,
                        PodcastChannelStatusId = (int)PodcastChannelStatusEnum.Published, // setting to "Published" status
                    };
                    await _podcastChannelStatusTrackingGenericRepository.CreateAsync(newPodcastChannelStatusTracking);

                    await transaction.CommitAsync();

                    var messageNextRequestData = command.RequestData;
                    messageNextRequestData["PodcastChannelId"] = podcastChannel.Id;
                    var messageResponseData = JObject.FromObject(new
                    {
                        PodcastChannelId = podcastChannel.Id,
                    }); 
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.ContentManagementDomain,
                        requestData: messageNextRequestData,
                        responseData: messageResponseData,
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "publish-channel.success"
                    );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();

                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.ContentManagementDomain,
                        requestData: command.RequestData,
                        responseData: JObject.FromObject(new
                        {
                            ErrorMessage = $"Publish podcast channel failed, error: {ex.Message}"
                        }),
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "publish-channel.failed"
                    );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);

                    Console.WriteLine("\n" + ex.StackTrace + "\n");
                }
            }

        }
    }
}