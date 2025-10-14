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
using UserService.BusinessLogic.DTOs.ViewModels.Mail;
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
using UserService.BusinessLogic.DTOs.MessageQueue.UserManagementDomain.CreatePodcastBuddyReview;

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

        private readonly HttpServiceQueryClient _httpServiceQueryClient;



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

        /////////////////////////////////////////////////////////////

        public async Task SendUserServiceEmail(MailProperty mailProperty, string toEmail, object viewModel)
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

                    var messageNextRequestData = JObject.FromObject(new
                    {
                        Email = existAccount.Email,
                        FullName = existAccount.FullName,
                        Dob = existAccount.Dob?.ToString("yyyy-MM-dd"),
                        Gender = existAccount.Gender,
                        Address = existAccount.Address,
                        Phone = existAccount.Phone,
                        MainImageFileKey = existAccount.MainImageFileKey,
                        RoleId = existAccount.RoleId,
                        Password = existAccount.Password,
                    });
                    var messageResponseData = JObject.FromObject(new
                    {
                        AccountId = existAccount.Id,
                        Message = "Register account successfully"
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
                    // throw new HttpRequestException("Đăng kí tài khoản thất bại, lỗi: " + ex.Message);
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
                            Account = new AccountSnippetDTO
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

        public async Task<List<PodcastBuddyListItemResponseDTO>> GetPodcastBuddyAccounts(int requestRoleId)
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
                if (requestRoleId == 1)
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
                            CommitmentDocumentFileKey = item.PodcasterProfile.CommitmentDocumentFileKey,
                            BuddyAudioFileKey = item.PodcasterProfile.BuddyAudioFileKey,
                            IsVerified = item.PodcasterProfile.IsVerified,
                        },
                        ReviewList = item.PodcastBuddyReviewPodcastBuddies?
                        .Where(r => r.Account != null).Select(r => new ReviewListItemDTO
                        {
                            Id = r.Id,
                            Account = new AccountSnippetDTO
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
                        Account = new AccountSnippetDTO
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
                        CommitmentDocumentFileKey = podcaster.PodcasterProfile.CommitmentDocumentFileKey,
                        BuddyAudioFileKey = podcaster.PodcasterProfile.BuddyAudioFileKey,
                        IsVerified = podcaster.PodcasterProfile.IsVerified,
                    },
                    ReviewList = podcaster.PodcastBuddyReviewPodcastBuddies?
                    .Where(r => r.Account != null).Select(r => new ReviewListItemDTO
                    {
                        Id = r.Id,
                        Account = new AccountSnippetDTO
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
                        IsVerified = account.IsVerified,
                        DeactivatedAt = account.DeactivatedAt,
                        RoleId = account.RoleId,
                        LastViolationLevelChanged = account.LastViolationLevelChanged,
                        LastViolationPointChanged = account.LastViolationPointChanged,
                        ViolationLevel = account.ViolationLevel,
                        ViolationPoint = account.ViolationPoint,
                        HasVerifiedPodcasterProfile = account.RoleId == 1 && account.PodcasterProfile != null && account.PodcasterProfile.IsVerified == true ? true : false
                    },
                    // set cache expiry to 1 hour (khi hết hạn key-value này sẽ bị xoá khỏi redis)
                    TimeSpan.FromHours(1)
                    );

                    if (result == false)
                    {
                        throw new Exception("Change account status failed, cannot update account status cache in redis");
                    }


                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.UserManagementDomain,
                        requestData: command.RequestData,
                        responseData: command.RequestData,
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
                    messageNextRequestData["CommitmentDocumentFileKey"] = podcasterProfile.CommitmentDocumentFileKey;

                    var messageResponseData = JObject.FromObject(new
                    {
                        Message = "Create podcaster profile successfully",
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

                    var folderPath = _filePathConfig.ACCOUNT_FILE_PATH + "\\" + podcasterProfile.AccountId;
                    if (updatePodcasterProfileParameterDTO.BuddyAudioFileKey != null && updatePodcasterProfileParameterDTO.BuddyAudioFileKey != "")
                    {
                        await _fileIOHelper.DeleteFileAsync(podcasterProfile.BuddyAudioFileKey);
                        var BuddyAudioFileKey = FilePathHelper.CombinePaths(folderPath, $"buddy_trailer_audio{FilePathHelper.GetExtension(updatePodcasterProfileParameterDTO.BuddyAudioFileKey)}");
                        await _fileIOHelper.CopyFileToFileAsync(updatePodcasterProfileParameterDTO.BuddyAudioFileKey, BuddyAudioFileKey);
                        await _fileIOHelper.DeleteFileAsync(updatePodcasterProfileParameterDTO.BuddyAudioFileKey);
                        podcasterProfile.BuddyAudioFileKey = BuddyAudioFileKey;

                    }

                    await _podcasterProfileGenericRepository.UpdateAsync(podcasterProfile.AccountId, podcasterProfile);
                    await transaction.CommitAsync();

                    var messageResponseData = JObject.FromObject(new
                    {
                        Message = "Update podcaster profile successfully",
                    });
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.UserManagementDomain,
                        requestData: command.RequestData,
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
                        await _fileIOHelper.DeleteFileAsync(account.MainImageFileKey);
                        var MainImageFileKey = FilePathHelper.CombinePaths(folderPath, $"main_image{FilePathHelper.GetExtension(updateUserInfo.MainImageFileKey)}");
                        await _fileIOHelper.CopyFileToFileAsync(updateUserInfo.MainImageFileKey, MainImageFileKey);
                        await _fileIOHelper.DeleteFileAsync(updateUserInfo.MainImageFileKey);
                        account.MainImageFileKey = MainImageFileKey;
                        await _accountGenericRepository.UpdateAsync(account.Id, account);
                    }

                    await transaction.CommitAsync();

                    var messageResponseData = JObject.FromObject(new
                    {
                        Message = "Update user successfully",
                    });
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.UserManagementDomain,
                        requestData: command.RequestData,
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

                    var messageResponseData = JObject.FromObject(new
                    {
                        Message = "Deactivate account successfully"
                    });
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.UserManagementDomain,
                        requestData: command.RequestData,
                        responseData: messageResponseData,
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "deactivate-account.success"
                    );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);
                    await SendChangeAccountStatusMessage(account.Id);
                    // [CHỈNH SỬA SAU] CHẠY CÁC FLOW XOÁ TRONG booking, chanel/show/episode (AccountFavoritedPodcastChannel/AccountFollowedPodcastShow/AccountSavedPodcastEpisode), podcast subscription, Report review session, publish review session, DMCA Accusation, AccountFollowedPodcaster

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

                    var messageResponseData = JObject.FromObject(new
                    {
                        Message = "Activate account successfully"
                    });
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.UserManagementDomain,
                        requestData: command.RequestData,
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

                    var messageResponseData = JObject.FromObject(new
                    {
                        Message = "Add account violation point successfully"
                    });
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.UserManagementDomain,
                        requestData: command.RequestData,
                        responseData: messageResponseData,
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "add-account-violation-point.success"
                    );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);
                    await SendChangeAccountStatusMessage(account.Id);
                    // [CHỈNH SỬA SAU] NẾU ACCOUNT VIOLATION LEVEL == MAX VIOLATION LEVEL THÌ CHẠY CÁC FLOW XOÁ TRONG booking, chanel/show/episode (AccountFavoritedPodcastChannel/AccountFollowedPodcastShow/AccountSavedPodcastEpisode), podcast subscription, Report review session, publish review session, DMCA Accusation, AccountFollowedPodcaster

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
                    await _podcasterProfileGenericRepository.UpdateAsync(podcasterProfile.AccountId, podcasterProfile);
                    await transaction.CommitAsync();
                    var messageResponseData = JObject.FromObject(new
                    {
                        Message = "Verify podcaster profile successfully",
                    });
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.UserManagementDomain,
                        requestData: command.RequestData,
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
                    var messageResponseData = JObject.FromObject(new
                    {
                        Message = "Create podcast buddy review successfully",
                    });
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.PublicReviewManagementDomain,
                        requestData: command.RequestData,
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
    }
}



