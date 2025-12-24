using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System.Security.Claims;
using Google.Apis.Auth;
using UserService.BusinessLogic.Helpers.AuthHelpers;
using UserService.Common.AppConfigurations.App.interfaces;
using UserService.Common.AppConfigurations.Jwt.interfaces;
using UserService.Infrastructure.Configurations.Google.interfaces;
using UserService.Common.AppConfigurations.FilePath.interfaces;
using UserService.DataAccess.Data;
using UserService.BusinessLogic.Helpers.FileHelpers;
using UserService.BusinessLogic.Helpers.DateHelpers;
using UserService.DataAccess.UOW;
using UserService.DataAccess.Repositories.interfaces;
using UserService.DataAccess.Entities.SqlServer;
using UserService.Infrastructure.Services.Google.Email;
using UserService.BusinessLogic.DTOs.MessageQueue.UserManagementDomain.VerifyAccount;
using UserService.Infrastructure.Services.Kafka;
using UserService.Infrastructure.Models.Kafka;
using UserService.BusinessLogic.Enums.Kafka;
using UserService.BusinessLogic.Services.MessagingServices.interfaces;
using UserService.BusinessLogic.DTOs.MessageQueue.UserManagementDomain.LoginAccountManual;
using UserService.BusinessLogic.DTOs.MessageQueue.UserManagementDomain.LoginAccountGoogle;
using UserService.BusinessLogic.Models.CrossService;
using UserService.BusinessLogic.Services.CrossServiceServices.QueryServices;
using UserService.BusinessLogic.Models.Mail;
using UserService.BusinessLogic.DTOs.MessageQueue.UserManagementDomain.SendResetPasswordLink;
using UserService.BusinessLogic.DTOs.MessageQueue.UserManagementDomain.NewResetPassword;
using Microsoft.EntityFrameworkCore;
using UserService.BusinessLogic.DTOs.Auth;
using System.Security.Cryptography;
using System.Text;
using UserService.BusinessLogic.DTOs.SystemConfiguration;
using UserService.BusinessLogic.DTOs.MessageQueue.UserManagementDomain.UpdateAccountPassword;

namespace UserService.BusinessLogic.Services.DbServices.UserServices
{
    public class AuthService
    {
        // LOGGER
        private readonly ILogger<AuthService> _logger;

        // CONFIG
        public readonly IAppConfig _appConfig;
        private readonly IJwtConfig _jwtConfig;
        private readonly IGoogleOAuth2Config _googleOAuth2Config;
        private readonly IGoogleMailConfig _googleMailConfig;
        private readonly IFilePathConfig _filePathConfig;

        // DB CONTEXT
        private readonly AppDbContext _appDbContext;

        // HELPERS
        private readonly BcryptHelper _bcryptHelper;
        private readonly JwtHelper _jwtHelper;
        private readonly DateHelper _dateHelpers;
        private readonly FileIOHelper _fileIOHelper;
        // UNIT OF WORK
        private readonly IUnitOfWork _unitOfWork;

        // REPOSITORIES
        private readonly IGenericRepository<Account> _accountGenericRepository;
        private readonly IGenericRepository<Role> _roleGenericRepository;
        private readonly IGenericRepository<PasswordResetToken> _passwordResetTokenGenericRepository;

        private readonly HttpServiceQueryClient _httpServiceQueryClient;


        // GOOGLE SERVICE
        private readonly FluentEmailService _fluentEmailService;

        // KAFKA SERVICE
        private readonly IMessagingService _messagingService;
        private readonly KafkaProducerService _kafkaProducerService;

        // DB SERVICES

        // RECORDS
        public record LoginRequestBody(string Email, string Password);


        public AuthService(
            ILogger<AuthService> logger,

            IAppConfig appConfig,
            IJwtConfig jwtConfig,
            IGoogleOAuth2Config googleOAuth2Config,
            IGoogleMailConfig googleMailConfig,
            IFilePathConfig filePathConfig,

            AppDbContext appDbContext,

            BcryptHelper bcryptHelper,
            JwtHelper jwtHelper,
            DateHelper dateHelpers,
            FileIOHelper fileIOHelper,

            IUnitOfWork unitOfWork,

            IGenericRepository<Account> accountGenericRepository,
            IGenericRepository<Role> roleGenericRepository,
            IGenericRepository<PasswordResetToken> passwordResetTokenGenericRepository,

            FluentEmailService fluentEmailService,

            HttpServiceQueryClient httpServiceQueryClient,
            IMessagingService messagingService,
            KafkaProducerService kafkaProducerService
            )
        {
            _logger = logger;

            _appConfig = appConfig;
            _jwtConfig = jwtConfig;
            _googleOAuth2Config = googleOAuth2Config;
            _googleMailConfig = googleMailConfig;
            _filePathConfig = filePathConfig;

            _appDbContext = appDbContext;

            _bcryptHelper = bcryptHelper;
            _jwtHelper = jwtHelper;
            _dateHelpers = dateHelpers;
            _fileIOHelper = fileIOHelper;

            _unitOfWork = unitOfWork;

            _accountGenericRepository = accountGenericRepository;
            _roleGenericRepository = roleGenericRepository;
            _passwordResetTokenGenericRepository = passwordResetTokenGenericRepository;

            _fluentEmailService = fluentEmailService;

            _httpServiceQueryClient = httpServiceQueryClient;
            _messagingService = messagingService;
            _kafkaProducerService = kafkaProducerService;

        }

        public async Task<SystemConfigProfileDTO> GetActiveSystemConfigProfile()
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

            // return ((JArray)result.Results["activeSystemConfigProfile"]).First as JObject;
            var config = (result.Results["activeSystemConfigProfile"].First as JObject).ToObject<SystemConfigProfileDTO>();
            return config;
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

        public static string SHA256Hash(string input)
        {
            using (var sha256 = SHA256.Create())
            {
                byte[] bytes = Encoding.UTF8.GetBytes(input);
                byte[] hash = sha256.ComputeHash(bytes);

                // Chuyển kết quả băm sang chuỗi dạng hex
                StringBuilder builder = new StringBuilder();
                foreach (byte b in hash)
                    builder.Append(b.ToString("x2"));

                return builder.ToString();
            }
        }
        public string ComputeFingerprint(DeviceInfoDTO info)
        {
            var data = $"{info.DeviceId}|{info.Platform}|{info.OSName}";
            return SHA256Hash(data);
        }

        /////////////////////////////////////////////////////////////
        public async Task LoginManual(LoginAccountManualParameterDTO loginData, SagaCommandMessage command)
        {
            var loginRequest = loginData;
            //             Login với account chưa verify -> login thất bại (tài khoản không tồn tại)
            // Login với account đã verify -> login bình thường
            // Login với account không tồn tại -> login thất bại (tài khoản không tồn tại)

            var account = await _unitOfWork.AccountRepository.FindByEmailAsync(loginRequest.Email,
            a => a.Include(ac => ac.Role)
             );
            if (account == null)
            {
                throw new HttpRequestException("Account does not exist");
            }
            if (account.DeactivatedAt != null && account.DeactivatedAt.Value < _dateHelpers.GetNowByAppTimeZone())
            {
                throw new HttpRequestException("Account has been deactivated");
            }

            if (account.IsVerified == false)
            {
                throw new HttpRequestException("Account has not been verified");
            }

            if (!_bcryptHelper.VerifyPassword(loginRequest.Password, account.Password))
            {
                throw new HttpRequestException("Email or password is incorrect");
            }

            try
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, account.Id.ToString()),
                    new Claim(ClaimTypes.Name, account.FullName),
                    new Claim(ClaimTypes.Email, account.Email),
                    new Claim("id", account.Id.ToString()),
                    new Claim("role_id", account.Role.Id.ToString()),
                    new Claim(ClaimTypes.Role, account.Role.Name),
                    new Claim("balance", account.Balance.ToString()),
                    new Claim("device_info_token", loginData.DeviceInfoToken ?? string.Empty),
                    new Claim(ClaimTypes.SerialNumber, Guid.NewGuid().ToString()), // Mã định danh JWT
                };

                var token = _jwtHelper.GenerateJWT_TwoPublicPrivateKey(claims, _jwtConfig.Exp);

                var user = _jwtHelper.DecodeToken_TwoPublicPrivateKey(token);

                // return JObject.FromObject(new
                // {
                //     Token = token,
                // });

                var messageNextRequestData = command.RequestData;
                messageNextRequestData["AccessToken"] = token;
                messageNextRequestData["Email"] = account.Email;
                messageNextRequestData["Password"] = loginRequest.Password;

                var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.UserManagementDomain,
                        requestData: messageNextRequestData,
                        responseData: JObject.FromObject(new
                        {
                            Message = "Login successful",
                            AccessToken = token,
                        }),
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "login-account-manual.success"
                        );
                await _messagingService.SendSagaMessageAsync(sagaEventMessage);
                await SendChangeAccountStatusMessage(account.Id);
            }
            catch (Exception ex)
            {
                var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                       topic: KafkaTopicEnum.UserManagementDomain,
                       requestData: command.RequestData,
                       responseData: JObject.FromObject(new
                       {
                           ErrorMessage = $"Login failed, error: {ex.Message}"
                       }),
                       sagaInstanceId: command.SagaInstanceId,
                       flowName: command.FlowName,
                       messageName: "login-account-manual.failed"
                       );
                await _messagingService.SendSagaMessageAsync(sagaEventMessage);
                Console.WriteLine("\n" + ex.StackTrace + "\n");
                // throw new HttpRequestException(ex.Message);
                // throw new HttpRequestException("Could not generate JWT");
            }


        }

        public async Task LoginGoogleAuthorizationCodeFlow(LoginAccountGoogleParameterDTO googleLoginData, SagaCommandMessage command)
        {
            string authorizationCode = googleLoginData.AuthorizationCode;
            string redirectUri = googleLoginData.RedirectUri;
            var clientId = _googleOAuth2Config.ClientId;
            var clientSecret = _googleOAuth2Config.ClientSecret;

            // 1. Gửi request đổi authorization code lấy access_token và id_token
            using var httpClient = new HttpClient();
            var tokenRequest = new HttpRequestMessage(HttpMethod.Post, "https://oauth2.googleapis.com/token");
            var tokenParams = new Dictionary<string, string>
            {
                { "code", authorizationCode },
                { "client_id", clientId },
                { "client_secret", clientSecret },
                { "redirect_uri", redirectUri },
                { "grant_type", "authorization_code" }
            };
            tokenRequest.Content = new FormUrlEncodedContent(tokenParams);
            var response = await httpClient.SendAsync(tokenRequest);
            if (!response.IsSuccessStatusCode)
                throw new UnauthorizedAccessException("Error exchanging authorization code for tokens");
            var payloadStr = await response.Content.ReadAsStringAsync();
            var payload = JObject.Parse(payloadStr);
            string idToken = payload["id_token"]?.ToString();
            if (string.IsNullOrEmpty(idToken))
                throw new UnauthorizedAccessException("ID Token missing in response");

            // 2. Xác thực id_token và trích xuất thông tin người dùng
            var googlePayload = await GoogleJsonWebSignature.ValidateAsync(idToken, new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = new[] { clientId }
            });
            if (googlePayload == null)
                throw new UnauthorizedAccessException("Invalid ID Token");

            // 3. Xử lý account
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var account = await _unitOfWork.AccountRepository.FindByEmailAsync(googlePayload.Email, a => a.Include(ac => ac.Role));
                    var activeSystemConfigProfile = await GetActiveSystemConfigProfile();

                    if (account == null)
                    {
                        // Chưa có account, tạo mới
                        var newpassword = Guid.NewGuid().ToString();
                        var newAccount = new Account
                        {
                            Email = googlePayload.Email,
                            Password = _bcryptHelper.HashPassword(newpassword),
                            FullName = googlePayload.Name ?? googlePayload.Email,
                            RoleId = 1,
                            IsVerified = true,
                            GoogleId = googlePayload.Subject,
                            // PodcastListenSlot = activeSystemConfigProfile["AccountConfig"].Value<int?>("PodcastListenSlotThreshold")
                            PodcastListenSlot = activeSystemConfigProfile.AccountConfig.PodcastListenSlotThreshold
                        };
                        newAccount = await _accountGenericRepository.CreateAsync(newAccount);

                        account = newAccount;
                        account.Role = await _roleGenericRepository.FindByIdAsync(newAccount.RoleId);

                        // gửi mail mật khẩu mới
                        var mailSendingRequestData = JObject.FromObject(new
                        {
                            SendUserServiceEmailMailInfo = new
                            {
                                // MailTypeName = "CustomerRegistrationVerification",
                                MailTypeName = "CustomerGoogleRegistrationNewAccountPassword",
                                ToEmail = newAccount.Email,
                                MailObject = new CustomerGoogleRegistrationNewAccountPasswordMailViewModel
                                {
                                    Email = newAccount.Email,
                                    FullName = newAccount.FullName ?? "",
                                    NewAccountPassword = newpassword
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
                    else
                    {
                        
                        // Đã có account
                        if (string.IsNullOrEmpty(account.GoogleId))
                        {
                            if (account.IsVerified == true)
                            {
                                account.GoogleId = googlePayload.Subject;
                                await _accountGenericRepository.UpdateAsync(account.Id, account);
                            }
                            else
                            {
                                await _accountGenericRepository.DeleteAsync(account.Id);
                                string newpassword = Guid.NewGuid().ToString();

                                var newAccount = new Account
                                {
                                    Email = googlePayload.Email,
                                    // Password = _bcryptHelper.HashPassword(Guid.NewGuid().ToString()),
                                    Password = _bcryptHelper.HashPassword(newpassword),
                                    FullName = googlePayload.Name ?? googlePayload.Email,
                                    RoleId = 1,
                                    IsVerified = true,
                                    GoogleId = googlePayload.Subject,
                                    // PodcastListenSlot = activeSystemConfigProfile["AccountConfig"].Value<int?>("PodcastListenSlotThreshold")
                                    PodcastListenSlot = activeSystemConfigProfile.AccountConfig.PodcastListenSlotThreshold
                                };
                                newAccount = await _accountGenericRepository.CreateAsync(newAccount);

                                account = newAccount;
                                account.Role = await _roleGenericRepository.FindByIdAsync(newAccount.RoleId);

                                // gửi mail mật khẩu mới
                                var mailSendingRequestData = JObject.FromObject(new
                                {
                                    SendUserServiceEmailMailInfo = new
                                    {
                                        // MailTypeName = "CustomerRegistrationVerification",
                                        MailTypeName = "CustomerGoogleRegistrationNewAccountPassword",
                                        ToEmail = newAccount.Email,
                                        MailObject = new CustomerGoogleRegistrationNewAccountPasswordMailViewModel
                                        {
                                            Email = newAccount.Email,
                                            FullName = newAccount.FullName ?? "",
                                            NewAccountPassword = newpassword
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
                        }
                        else if (account.GoogleId != googlePayload.Subject)
                        {
                            throw new HttpRequestException("Account is associated with a different Google account");
                        }
                        else if (account.GoogleId == googlePayload.Subject && !account.IsVerified)
                        {
                            throw new HttpRequestException("This Google account has not been activated");
                        }
                        // else: GoogleId trùng và đã verify => thành công
                    }

                    // Check deactivated
                    if (account.DeactivatedAt != null && account.DeactivatedAt.Value < _dateHelpers.GetNowByAppTimeZone())
                    {
                        throw new HttpRequestException("Account has been deactivated");
                    }

                    // Tạo claims và trả về giống LoginManual
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.NameIdentifier, account.Id.ToString()),
                        new Claim(ClaimTypes.Name, account.FullName ?? string.Empty),
                        new Claim(ClaimTypes.Email, account.Email),
                        new Claim("id", account.Id.ToString()),
                        new Claim("role_id", account.RoleId.ToString()),
                        new Claim(ClaimTypes.Role, account.Role.Name),
                        new Claim("balance", account.Balance.ToString()),
                        new Claim("device_info_token", googleLoginData.DeviceInfoToken ?? string.Empty),
                        new Claim(ClaimTypes.SerialNumber, Guid.NewGuid().ToString()),
                    };
                    var token = _jwtHelper.GenerateJWT_TwoPublicPrivateKey(claims, _jwtConfig.Exp);
                    await transaction.CommitAsync();
                    // return JObject.FromObject(new { Token = token });
                    var messageNextRequestData = command.RequestData;
                    messageNextRequestData["AuthorizationCode"] = authorizationCode;
                    messageNextRequestData["RedirectUri"] = redirectUri;
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                            topic: KafkaTopicEnum.UserManagementDomain,
                            requestData: messageNextRequestData,
                            responseData: JObject.FromObject(new
                            {
                                Message = "Login successful",
                                AccessToken = token,
                            }),
                            sagaInstanceId: command.SagaInstanceId,
                            flowName: command.FlowName,
                            messageName: "login-account-google.success"
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
                           ErrorMessage = $"Google Login failed, error: {ex.Message}"
                       }),
                       sagaInstanceId: command.SagaInstanceId,
                       flowName: command.FlowName,
                       messageName: "login-account-google.failed"
                       );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);

                    Console.WriteLine("\n" + ex.StackTrace + "\n");
                    // throw new HttpRequestException(ex.Message);
                    // throw new UnauthorizedAccessException("Lỗi đăng nhập bằng Google");
                }
            }
        }

        public async Task AccountVerification(VerifyAccountParameterDTO verificationData, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var account = await _unitOfWork.AccountRepository.FindByEmailAsync(verificationData.Email);
                    if (account == null)
                    {
                        // throw new Exception("không tìm thấy tài khoản với email: " + verificationData.Email);
                        throw new HttpRequestException("Account with email does not exist: " + verificationData.Email);
                    }
                    if (account.DeactivatedAt != null && account.DeactivatedAt.Value < _dateHelpers.GetNowByAppTimeZone())
                    {
                        throw new HttpRequestException("Account has been deactivated");
                    }
                    if (account.VerifyCode != verificationData.VerifyCode)
                    {
                        throw new HttpRequestException("Verification code is incorrect");
                    }
                    account.IsVerified = true;
                    account.VerifyCode = null;

                    await _accountGenericRepository.UpdateAsync(account.Id, account);

                    await transaction.CommitAsync();

                    var messageNextRequestData = command.RequestData;
                    messageNextRequestData["Email"] = account.Email;
                    messageNextRequestData["IsVerified"] = account.IsVerified;

                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.UserManagementDomain,
                        requestData: messageNextRequestData,
                        responseData: JObject.FromObject(new
                        {
                            // Message = "Account verification successful",
                            Email = account.Email,
                            IsVerified = account.IsVerified
                        }),
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "verify-account.success"
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
                           ErrorMessage = $"Account verification failed, error: {ex.Message}"
                       }),
                       sagaInstanceId: command.SagaInstanceId,
                       flowName: command.FlowName,
                       messageName: "verify-account.failed"
                       );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);
                    Console.WriteLine("\n" + ex.StackTrace + "\n");
                    // throw new HttpRequestException("Xác thực tài khoản thất bại, lỗi: " + ex.Message);
                }
            }
        }



        public async Task ForgotPassword(SendResetPasswordLinkParameterDTO forgotPasswordData, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var account = await _unitOfWork.AccountRepository.FindByEmailAsync(forgotPasswordData.Email);
                    if (account == null)
                    {
                        throw new HttpRequestException("Account does not exist");
                    }
                    if (account.IsVerified == false)
                    {
                        throw new HttpRequestException("Account has not been verified");
                    }
                    if (account.DeactivatedAt != null && account.DeactivatedAt.Value < _dateHelpers.GetNowByAppTimeZone())
                    {
                        throw new HttpRequestException("Account has been deactivated");
                    }

                    string NewGuid = Guid.NewGuid().ToString();
                    await _unitOfWork.PasswordResetTokenRepository.CreateAsync(new PasswordResetToken
                    {
                        AccountId = account.Id,
                        Token = NewGuid,
                        ExpiredAt = _dateHelpers.GetNowByAppTimeZone().AddHours(_appConfig.RESET_PASSWORD.TokenExpiredInMinutes),
                        IsUsed = false,
                        CreatedAt = _dateHelpers.GetNowByAppTimeZone()
                    });
                    string resetPasswordUrl = _appConfig.RESET_PASSWORD.Url + "?email=" + forgotPasswordData.Email + "&token=" + NewGuid;
                    Console.WriteLine("Reset Password URL: " + resetPasswordUrl);

                    // await _fluentEmailService.SendEmail(email, new ForgotPasswordEmailViewModel
                    // {
                    //     Email = email,
                    //     PasswordResetToken = NewGuid,
                    //     ResetPasswordUrl = resetPasswordUrl,
                    //     ExpiredAt = _dateHelpers.GetNowByAppTimeZone().AddHours(1).ToString("dd/MM/yyyy HH:mm:ss")
                    // }, _googleMailConfig.AccountForgotPassword_TemplateViewPath
                    // , _googleMailConfig.AccountForgotPassword_MailSubject);

                    // var mailSendingRequestData = JObject.FromObject(new
                    // {
                    //     MailTypeName = "CustomerPasswordReset",
                    //     ToEmail = forgotPasswordData.Email,
                    //     MailObject = new CustomerPasswordResetMailViewModel
                    //     {
                    //         Email = forgotPasswordData.Email,
                    //         PasswordResetToken = NewGuid,
                    //         ResetPasswordUrl = resetPasswordUrl,
                    //         ExpiredAt = _dateHelpers.GetNowByAppTimeZone().AddHours(1).ToString("dd/MM/yyyy HH:mm:ss"),
                    //     }
                    // });
                    var mailSendingRequestData = JObject.FromObject(new
                    {
                        SendUserServiceEmailMailInfo = new
                        {
                            MailTypeName = "CustomerPasswordReset",
                            ToEmail = forgotPasswordData.Email,
                            MailObject = JObject.FromObject(new CustomerPasswordResetMailViewModel
                            {
                                Email = forgotPasswordData.Email,
                                PasswordResetToken = NewGuid,
                                ResetPasswordUrl = resetPasswordUrl,
                                ExpiredAt = _dateHelpers.GetNowByAppTimeZone().AddHours(1).ToString("dd/MM/yyyy HH:mm:ss"),
                            })
                        }
                    });
                    var mailSendingFlow = _kafkaProducerService.PrepareStartSagaTriggerMessage(
                        topic: KafkaTopicEnum.UserManagementDomain,
                        requestData: mailSendingRequestData,
                        sagaInstanceId: null,
                        messageName: "user-service-mail-sending-flow");
                    await _messagingService.SendSagaMessageAsync(mailSendingFlow);

                    await transaction.CommitAsync();

                    // var requestData = JObject.FromObject(new CustomerPasswordResetMailViewModel
                    // {
                    //     Email = forgotPasswordData.Email,
                    //     PasswordResetToken = NewGuid,
                    //     ResetPasswordUrl = resetPasswordUrl,
                    //     ExpiredAt = _dateHelpers.GetNowByAppTimeZone().AddHours(1).ToString("dd/MM/yyyy HH:mm:ss"),
                    // });

                    var messageNextRequestData = command.RequestData;
                    messageNextRequestData["Email"] = account.Email;
                    messageNextRequestData["PasswordResetToken"] = NewGuid;
                    messageNextRequestData["ResetPasswordUrl"] = resetPasswordUrl;
                    messageNextRequestData["ExpiredAt"] = _dateHelpers.GetNowByAppTimeZone().AddHours(1).ToString("dd/MM/yyyy HH:mm:ss");

                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.UserManagementDomain,
                        requestData: messageNextRequestData,
                        responseData: JObject.FromObject(new
                        {
                            // Message = "Forgot password process initiated. Please check your email for the reset link."
                            Email = account.Email,
                            PasswordResetToken = NewGuid,
                            ResetPasswordUrl = resetPasswordUrl,
                            ExpiredAt = _dateHelpers.GetNowByAppTimeZone().AddHours(1).ToString("dd/MM/yyyy HH:mm:ss"),
                        }),
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "send-reset-password-link.success"
                        );

                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);
                }
                catch (HttpRequestException ex)
                {
                    await transaction.RollbackAsync();
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                       topic: KafkaTopicEnum.UserManagementDomain,
                       requestData: command.RequestData,
                       responseData: JObject.FromObject(new
                       {
                           ErrorMessage = $"Forgot password process failed, error: {ex.Message}"
                       }),
                       sagaInstanceId: command.SagaInstanceId,
                       flowName: command.FlowName,
                       messageName: "send-reset-password-link.failed"
                       );

                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);
                    Console.WriteLine("\n" + ex.StackTrace + "\n");
                    // throw new HttpRequestException(ex.Message);
                }
            }

        }


        public async Task ResetPassword(NewResetPasswordParameterDTO newResetPasswordRequest, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var account = await _unitOfWork.AccountRepository.FindByEmailAsync(newResetPasswordRequest.Email);
                    if (account == null)
                    {
                        throw new HttpRequestException("Account does not exist");
                    }
                    if (account.DeactivatedAt != null && account.DeactivatedAt.Value < _dateHelpers.GetNowByAppTimeZone())
                    {
                        throw new HttpRequestException("Account has been deactivated");
                    }

                    var passwordResetToken = await this._unitOfWork.PasswordResetTokenRepository.getValidToken(account.Id, newResetPasswordRequest.ResetPasswordToken);

                    account.Password = _bcryptHelper.HashPassword(newResetPasswordRequest.NewPassword);
                    await _accountGenericRepository.UpdateAsync(account.Id, account);

                    passwordResetToken.IsUsed = true;
                    await _passwordResetTokenGenericRepository.UpdateAsync(passwordResetToken.Id, passwordResetToken);

                    await transaction.CommitAsync();

                    var messageNextRequestData = command.RequestData;
                    messageNextRequestData["Email"] = account.Email;
                    messageNextRequestData["NewPassword"] = newResetPasswordRequest.NewPassword;
                    messageNextRequestData["ResetPasswordToken"] = newResetPasswordRequest.ResetPasswordToken;

                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.UserManagementDomain,
                        requestData: messageNextRequestData,
                        responseData: JObject.FromObject(new
                        {
                            // Message = "Password has been reset successfully"
                            Email = account.Email,
                            NewPassword = newResetPasswordRequest.NewPassword,
                            ResetPasswordToken = newResetPasswordRequest.ResetPasswordToken
                        }),
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "reset-account-password.success"
                        );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);
                    await SendChangeAccountStatusMessage(account.Id);
                }
                catch (HttpRequestException ex)
                {
                    await transaction.RollbackAsync();
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                       topic: KafkaTopicEnum.UserManagementDomain,
                       requestData: command.RequestData,
                       responseData: JObject.FromObject(new
                       {
                           ErrorMessage = $"Reset password process failed, error: {ex.Message}"
                       }),
                       sagaInstanceId: command.SagaInstanceId,
                       flowName: command.FlowName,
                       messageName: "reset-account-password.failed"
                       );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);
                    Console.WriteLine("\n" + ex.StackTrace + "\n");
                }
            }
        }

        public async Task UpdateAccountPassword(UpdateAccountPasswordParameterDTO updateAccountPasswordParameterDTO, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var account = await _accountGenericRepository.FindByIdAsync(updateAccountPasswordParameterDTO.AccountId);
                    if (account == null)
                    {
                        throw new HttpRequestException("Account does not exist");
                    }
                    else if (account.IsVerified == false)
                    {
                        throw new HttpRequestException("Account has not been verified");
                    }
                    if (account.DeactivatedAt != null)
                    {
                        throw new HttpRequestException("Account has been deactivated");
                    }
                    if (!_bcryptHelper.VerifyPassword(updateAccountPasswordParameterDTO.OldPassword, account.Password))
                    {
                        throw new HttpRequestException("Old password is incorrect");
                    }

                    account.Password = _bcryptHelper.HashPassword(updateAccountPasswordParameterDTO.NewPassword);
                    await _accountGenericRepository.UpdateAsync(account.Id, account);

                    await transaction.CommitAsync();

                    var messageNextRequestData = command.RequestData;
                    messageNextRequestData["AccountId"] = account.Id;
                    messageNextRequestData["OldPassword"] = updateAccountPasswordParameterDTO.OldPassword;
                    messageNextRequestData["NewPassword"] = updateAccountPasswordParameterDTO.NewPassword;

                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.UserManagementDomain,
                        requestData: messageNextRequestData,
                        responseData: JObject.FromObject(new
                        {
                            // Message = "Password has been updated successfully"
                            AccountId = account.Id,
                            OldPassword = updateAccountPasswordParameterDTO.OldPassword,
                            NewPassword = updateAccountPasswordParameterDTO.NewPassword
                        }),
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "update-account-password.success"
                        );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);
                }
                catch (HttpRequestException ex)
                {
                    await transaction.RollbackAsync();
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                       topic: KafkaTopicEnum.UserManagementDomain,
                       requestData: command.RequestData,
                       responseData: JObject.FromObject(new
                       {
                           ErrorMessage = $"Update password process failed, error: {ex.Message}"
                       }),
                       sagaInstanceId: command.SagaInstanceId,
                       flowName: command.FlowName,
                       messageName: "update-account-password.failed"
                       );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);
                    Console.WriteLine("\n" + ex.StackTrace + "\n");
                }
            }
        }
    }
}
