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
using UserService.DataAccess.Entities.SqlServer;
using UserService.BusinessLogic.Services.CrossServiceServices.QueryServices;
using UserService.BusinessLogic.Models.CrossService;
using Newtonsoft.Json.Linq;
using UserService.Infrastructure.Services.Kafka;
using UserService.BusinessLogic.Enums.Kafka;
using UserService.Infrastructure.Models.Kafka;
using UserService.BusinessLogic.Services.MessagingServices.interfaces;
using Confluent.Kafka;
using UserService.BusinessLogic.DTOs.MessageQueue.UserManagementDomain.SendUserServiceEmail;

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

        private readonly HttpServiceQueryClient _httpServiceQueryClient;



        // GOOGLE SERVICE
        private readonly FluentEmailService _fluentEmailService;

        // KAFKA SERVICE
        private readonly IMessagingService _messagingService;
        private readonly KafkaProducerService _kafkaProducerService;

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

            FileIOHelper fileIOHelper,
            IFilePathConfig filePathConfig,
            IGoogleMailConfig googleMailConfig,
            IAppConfig appConfig,
            IAccountConfig accountConfig,

            HttpServiceQueryClient httpServiceQueryClient,
            IMessagingService messagingService,
            KafkaProducerService kafkaProducerService
            )
        {
            _logger = logger;

            _appDbContext = appDbContext;
            _unitOfWork = unitOfWork;

            _accountGenericRepository = accountGenericRepository;
            _roleGenericRepository = roleGenericRepository;

            _fileIOHelper = fileIOHelper;
            _jwtHelper = jwtHelper;
            _bcryptHelper = bcryptHelper;

            _fluentEmailService = fluentEmailService;

            _filePathConfig = filePathConfig;
            _accountConfig = accountConfig;
            _googleMailConfig = googleMailConfig;
            _appConfig = appConfig;

            _httpServiceQueryClient = httpServiceQueryClient;
            _messagingService = messagingService;
            _kafkaProducerService = kafkaProducerService;
        }

        public async Task<Account> GetExistAccountById(int accountId)
        {
            // var account = await _unitOfWork.AccountRepository.FindByIdAsync(accountId);
            var account = await _accountGenericRepository.FindByIdAsync(accountId);
            if (account == null)
            {
                throw new Exception("không tìm thấy tài khoản có id " + accountId);
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
                var customers = await _unitOfWork.AccountRepository.FindByRoleIdAsync(1, null, a => a.Role);
                if (customers == null || !customers.Any())
                {
                    throw new Exception("No customer accounts found");
                }

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
                , a => a.Role);
                if (staffs == null || !staffs.Any())
                {
                    throw new Exception("No staff accounts found");
                }

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



        // public async Task RegisterStaff(StaffRegisterDTO staffRegisterInfo)
        // {
        //     using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
        //     {
        //         try
        //         {
        //             var registerInfo = staffRegisterInfo.RegisterInfo;
        //             var staff = await _unitOfWork.AccountRepository.FindByEmailAsync(registerInfo.Email);
        //             if (staff != null)
        //             {
        //                 throw new Exception("email đã tồn tại: " + registerInfo.Email);
        //             }
        //             var role = await this.GetExistRoleById(registerInfo.RoleId);

        //             staff = new Account
        //             {
        //                 Email = registerInfo.Email,
        //                 Password = _bcryptHelpers.HashPassword(registerInfo.Password),
        //                 FullName = registerInfo.FullName,
        //                 RoleId = registerInfo.RoleId,
        //                 Dob = DateOnly.FromDateTime(registerInfo.Dob),
        //                 Gender = registerInfo.Gender,
        //                 Address = registerInfo.Address,
        //                 Phone = registerInfo.Phone,
        //                 IsFilterSurveyRequired = false,
        //                 IsVerified = true,
        //             };
        //             await _accountGenericRepository.CreateAsync(staff);

        //             var folderPath = _filePathConfig.ACCOUNt_IMAGE_PATH + "\\" + staff.Id;
        //             if (registerInfo.ImageBase64 != null && registerInfo.ImageBase64 != "")
        //             {
        //                 string fileName = "main";
        //                 string base64Data = registerInfo.ImageBase64;

        //                 await _imageHelpers.SaveBase64File(base64Data, folderPath, fileName);
        //             }
        //             else
        //             {
        //                 await _imageHelpers.CopyFile(_filePathConfig.ACCOUNt_IMAGE_PATH, "unknown", folderPath, "main");
        //             }

        //             await transaction.CommitAsync();

        //         }
        //         catch (Exception ex)
        //         {
        //             await transaction.RollbackAsync();
        //             Console.WriteLine("\n" + ex.StackTrace + "\n");
        //             throw new HttpRequestException("Đăng kí tài khoản thất bại, lỗi: " + ex.Message);
        //         }
        //     }

        // }


        // public async Task<AccountDetailDTO> GetAccountById(int accountId)
        // {
        //     try
        //     {
        //         var account = await this.GetExistAccountById(accountId);
        //         return new AccountDetailDTO
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
        //             Xp = account.Xp,
        //             Level = account.Level,
        //             ProgressionSurveyCount = account.ProgressionSurveyCount,
        //             IsFilterSurveyRequired = account.IsFilterSurveyRequired,
        //             LastFilterSurveyTakenAt = account.LastFilterSurveyTakenAt?.ToString(),
        //             DeactivatedAt = account.DeactivatedAt?.ToString(),
        //             CreatedAt = account.CreatedAt.ToString(),
        //             UpdatedAt = account.UpdatedAt.ToString(),
        //             MainImageUrl = await _imageHelpers.GenerateImageUrl(_filePathConfig.ACCOUNt_IMAGE_PATH, account.Id.ToString(), "main"),
        //             Profile = new AccountProfileDTO
        //             {
        //                 CountryRegion = account.AccountProfile?.CountryRegion,
        //                 MaritalStatus = account.AccountProfile?.MaritalStatus,
        //                 AverageIncome = account.AccountProfile?.AverageIncome,
        //                 EducationLevel = account.AccountProfile?.EducationLevel,
        //                 JobField = account.AccountProfile?.JobField,
        //                 ProvinceCode = account.AccountProfile?.ProvinceCode,
        //                 DistrictCode = account.AccountProfile?.DistrictCode,
        //                 WardCode = account.AccountProfile?.WardCode
        //             }
        //         };
        //     }
        //     catch (Exception ex)
        //     {
        //         Console.WriteLine("\n" + ex.StackTrace + "\n");
        //         throw new HttpRequestException("Lấy thông tin tài khoản thất bại, lỗi: " + ex.Message);
        //     }


        // }

        // public async Task<AccountDetailDTO> GetMe(int accountId)
        // {
        //     try
        //     {
        //         var account = await this.GetExistAccountById(accountId);
        //         return new AccountDetailDTO
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
        //             Xp = account.Xp,
        //             Level = account.Level,
        //             ProgressionSurveyCount = account.ProgressionSurveyCount,
        //             IsFilterSurveyRequired = account.IsFilterSurveyRequired,
        //             LastFilterSurveyTakenAt = account.LastFilterSurveyTakenAt?.ToString(),
        //             DeactivatedAt = account.DeactivatedAt?.ToString(),
        //             CreatedAt = account.CreatedAt.ToString(),
        //             UpdatedAt = account.UpdatedAt.ToString(),
        //             MainImageUrl = await _imageHelpers.GenerateImageUrl(_filePathConfig.ACCOUNt_IMAGE_PATH, account.Id.ToString(), "main"),
        //             Profile = new AccountProfileDTO
        //             {
        //                 CountryRegion = account.AccountProfile?.CountryRegion,
        //                 MaritalStatus = account.AccountProfile?.MaritalStatus,
        //                 AverageIncome = account.AccountProfile?.AverageIncome,
        //                 EducationLevel = account.AccountProfile?.EducationLevel,
        //                 JobField = account.AccountProfile?.JobField,
        //                 ProvinceCode = account.AccountProfile?.ProvinceCode,
        //                 DistrictCode = account.AccountProfile?.DistrictCode,
        //                 WardCode = account.AccountProfile?.WardCode
        //             },
        //             IsPlatformFeedbackGiven = account.PlatformFeedback != null
        //         };
        //     }
        //     catch (Exception ex)
        //     {
        //         Console.WriteLine("\n" + ex.StackTrace + "\n");
        //         throw new HttpRequestException("Lấy thông tin tài khoản thất bại, lỗi: " + ex.Message);
        //     }


        // }

        // public async Task<Account> UpdateAccount(int accountId, AccountUpdateDTO accountUpdateDto)
        // {
        //     using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
        //     {
        //         try
        //         {
        //             var account = await this.GetExistAccountById(accountId);

        //             // Cập nhật thông tin tài khoản
        //             account.FullName = accountUpdateDto.FullName;
        //             account.Dob = accountUpdateDto.Dob;
        //             account.Gender = accountUpdateDto.Gender;
        //             account.Address = accountUpdateDto.Address;
        //             account.Phone = accountUpdateDto.Phone;

        //             await _accountGenericRepository.UpdateAsync(account.Id, account);

        //             // Cập nhật ảnh đại diện
        //             var folderPath = _filePathConfig.ACCOUNt_IMAGE_PATH + "\\" + account.Id;
        //             if (accountUpdateDto.ImageBase64 != null && accountUpdateDto.ImageBase64 != "")
        //             {
        //                 string fileName = "main";
        //                 string base64Data = accountUpdateDto.ImageBase64;

        //                 await _imageHelpers.SaveBase64File(base64Data, folderPath, fileName);
        //             }
        //             await transaction.CommitAsync();
        //             return account;

        //         }
        //         catch (Exception ex)
        //         {
        //             await transaction.RollbackAsync();
        //             Console.WriteLine("\n" + ex.StackTrace + "\n");
        //             throw new HttpRequestException("Cập nhật tài khoản thất bại, lỗi: " + ex.Message);
        //         }
        //     }
        // }

        // public async Task UpdateAccountProfile(int accountId, AccountProfileUpdateDTO accountUpdateProfileDto)
        // {
        //     using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
        //     {
        //         try
        //         {
        //             var account = await this.GetExistAccountById(accountId);

        //             var accountProfile = await this.GetExistAccountProfileByAccountId(accountId);
        //             // Cập nhật thông tin tài khoản
        //             accountProfile.CountryRegion = accountUpdateProfileDto.AccountProfile.CountryRegion;
        //             accountProfile.MaritalStatus = accountUpdateProfileDto.AccountProfile.MaritalStatus;
        //             accountProfile.AverageIncome = accountUpdateProfileDto.AccountProfile.AverageIncome;
        //             accountProfile.EducationLevel = accountUpdateProfileDto.AccountProfile.EducationLevel;
        //             accountProfile.JobField = accountUpdateProfileDto.AccountProfile.JobField;
        //             accountProfile.ProvinceCode = accountUpdateProfileDto.AccountProfile.ProvinceCode;
        //             accountProfile.DistrictCode = accountUpdateProfileDto.AccountProfile.DistrictCode;
        //             accountProfile.WardCode = accountUpdateProfileDto.AccountProfile.WardCode;
        //             await _unitOfWork.AccountProfileRepository.UpdateAsync(accountProfile);
        //             // Cập nhật sở thích chủ đề khảo sát
        //             if (accountUpdateProfileDto.SurveyTopicFavorites != null && accountUpdateProfileDto.SurveyTopicFavorites.Any())
        //             {
        //                 // Xoá tất cả sở thích cũ
        //                 await this._unitOfWork.SurveyTopicFavoriteRepository.DeleteByAccountIdAsync(accountId);

        //                 // Thêm sở thích mới
        //                 foreach (var favoriteDto in accountUpdateProfileDto.SurveyTopicFavorites)
        //                 {

        //                     var surveyTopicFavorite = new SurveyTopicFavorite
        //                     {
        //                         AccountId = accountId,
        //                         SurveyTopicId = favoriteDto.SurveyTopicId,
        //                         FavoriteScore = favoriteDto.FavoriteScore
        //                     };
        //                     await _surveyTopicFavoriteGenericRepository.CreateAsync(surveyTopicFavorite);
        //                 }
        //             }


        //             await transaction.CommitAsync();
        //         }
        //         catch (Exception ex)
        //         {
        //             await transaction.RollbackAsync();
        //             Console.WriteLine("\n" + ex.StackTrace + "\n");
        //             throw new HttpRequestException("Cập nhật thông tin tài khoản thất bại, lỗi: " + ex.Message);
        //         }
        //     }
        // }

        // public async Task DeactivateAccount(int accountId, bool isDeactivate)
        // {
        //     using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
        //     {
        //         try
        //         {
        //             var account = await this.GetExistAccountById(accountId);
        //             // if (isDeactivate == false)
        //             // {
        //             //     account.DeactivatedAt = null;
        //             // }else
        //             // {
        //             //     account.DeactivatedAt = this._dateHelpers.GetNowByAppTimeZone();
        //             // }

        //             await this._unitOfWork.AccountRepository.DeactivateAsync(account.Id, isDeactivate);
        //             // Cập nhật thông tin tài khoản

        //             await transaction.CommitAsync();
        //         }
        //         catch (Exception ex)
        //         {
        //             await transaction.RollbackAsync();
        //             Console.WriteLine("\n" + ex.StackTrace + "\n");
        //             throw new HttpRequestException("Vô hiệu hoá tài khoản thất bại, lỗi: " + ex.Message);
        //         }
        //     }
        // }

        // public async Task AccountLvlXPDeduction()
        // {
        //     using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
        //     {
        //         try
        //         {
        //             var config = await _unitOfWork.SystemConfigProfileRepository.FindActiveProfileAsync();
        //             var accountGeneralConfig = config.AccountGeneralConfig;
        //             var accountLevelSettingConfigs = config.AccountLevelSettingConfigs.ToList();
        //             var accounts = await _accountGenericRepository.FindAll(
        //                 predicate: account => account.DeactivatedAt == null && account.IsVerified == true && account.RoleId == 4 && account.Level > 1 && account.Xp > 0,
        //                 includeProperties: account => account.AccountProfile
        //                 ).ToListAsync();

        //             foreach (var account in accounts)
        //             {
        //                 account.Xp -= accountLevelSettingConfigs.Where(x => x.Level == account.Level).First().DailyReductionXp;
        //                 //account.Xp -= accountLevelSettingConfigs[account.Level - 1].DailyReductionXp; Use this for performance but only if the list is in order of level
        //                 if (account.Xp < account.Level * accountGeneralConfig.XpLevelThreshold)
        //                 {
        //                     account.ProgressionSurveyCount = 0;
        //                     account.Level = (int)Math.Floor((decimal)account.Xp / (decimal)accountGeneralConfig.XpLevelThreshold);
        //                 }
        //                 await _accountGenericRepository.UpdateAsync(account.Id, account);
        //             }
        //             await transaction.CommitAsync();
        //         }
        //         catch (Exception ex)
        //         {
        //             await transaction.RollbackAsync();
        //             Console.WriteLine("\n" + ex.StackTrace + "\n");
        //             throw new HttpRequestException("Giảm Xp hàng ngày của account thất bại, lỗi: " + ex.Message);
        //         }
        //     }
        // }

        // public async Task AccountFilterSurveyRequiredChecking()
        // {
        //     using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
        //     {
        //         try
        //         {
        //             var config = await _unitOfWork.SystemConfigProfileRepository.FindActiveProfileAsync();
        //             var accountGeneralConfig = config.AccountGeneralConfig;
        //             var accountLevelSettingConfigs = config.AccountLevelSettingConfigs.ToList();
        //             var accounts = await _accountGenericRepository.FindAll(
        //                 predicate: account => account.DeactivatedAt == null && account.IsVerified == true && account.RoleId == 4 && !account.IsFilterSurveyRequired,
        //                 includeProperties: account => account.AccountProfile
        //                 ).ToListAsync();

        //             foreach (var account in accounts)
        //             {
        //                 var timeCheck = _dateHelpers.GetNowByAppTimeZone().AddDays(-accountGeneralConfig.FilterSurveyCycle);
        //                 if (timeCheck >= account.LastFilterSurveyTakenAt)
        //                 {
        //                     account.IsFilterSurveyRequired = true;
        //                 }
        //                 else
        //                 {
        //                     account.IsFilterSurveyRequired = false;
        //                 }

        //                 await _accountGenericRepository.UpdateAsync(account.Id, account);
        //             }
        //             await transaction.CommitAsync();
        //         }
        //         catch (Exception ex)
        //         {
        //             await transaction.RollbackAsync();
        //             Console.WriteLine("\n" + ex.StackTrace + "\n");
        //             throw new HttpRequestException("Giảm Xp hàng ngày của account thất bại, lỗi: " + ex.Message);
        //         }
        //     }
        // }





    }
}
