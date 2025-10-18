using Microsoft.Extensions.Logging;
using SystemConfigurationService.Common.AppConfigurations.App.interfaces;
using SystemConfigurationService.Common.AppConfigurations.Jwt.interfaces;
using SystemConfigurationService.DataAccess.Data;
using SystemConfigurationService.DataAccess.UOW;
using SystemConfigurationService.DataAccess.Repositories.interfaces;
using SystemConfigurationService.DataAccess.Entities;
using Newtonsoft.Json.Linq;
using System.Security.Claims;
using SystemConfigurationService.BusinessLogic.DTOs.Auth;
using SystemConfigurationService.BusinessLogic.DTOs.ViewModels.Mail;
using SystemConfigurationService.Common.AppConfigurations.FilePath.interfaces;
using Google.Apis.Auth;
using SystemConfigurationService.Common.AppConfigurations.BusinessSetting.interfaces;
using SystemConfigurationService.BusinessLogic.Services.DbServices.FilterServices;
using SystemConfigurationService.Infrastructure.Services.Google.Email;
using SystemConfigurationService.Infrastructure.Configurations.Google.interfaces;
using SystemConfigurationService.BusinessLogic.Helpers.AuthHelpers;
using SystemConfigurationService.BusinessLogic.Helpers.FileHelpers;
using SystemConfigurationService.BusinessLogic.Helpers.DateHelpers;

namespace SystemConfigurationService.BusinessLogic.Services.DbServices.SystemConfigurationServices
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
        private readonly IAccountConfig _accountConfig;

        // DB CONTEXT
        private readonly AppDbContext _appDbContext;

        // HELPERS
        private readonly BcryptHelper _bcryptHelper;
        private readonly JwtHelper _jwtHelper;
        private readonly DateHelper _dateHelper;
        private readonly FileIOHelper _fileIOHelper;

        // UNIT OF WORK
        private readonly IUnitOfWork _unitOfWork;

        // REPOSITORIES
        private readonly IGenericRepository<Account> _accountGenericRepository;
        private readonly IGenericRepository<Role> _roleGenericRepository;
        private readonly IGenericRepository<PasswordResetToken> _passwordResetTokenGenericRepository;
        private readonly IGenericRepository<AccountProfile> _accountProfileGenericRepository;


        // GOOGLE SERVICE
        private readonly FluentEmailService _fluentEmailService;

        // DB SERVICES
        private readonly FilterTagService _filterTagService;

        // RECORDS
        public record LoginRequestBody(string Email, string Password);


        public AuthService(ILogger<AuthService> logger, AppDbContext appDbContext, BcryptHelper bcryptHelper, JwtHelper jwtHelper,
            IUnitOfWork unitOfWork,
            IJwtConfig jwtConfig,
            IFilePathConfig filePathConfig,
            IAppConfig appConfig,
            IGoogleOAuth2Config googleOAuth2Config,
            IAccountConfig accountConfig,
            IGoogleMailConfig googleMailConfig,

            IGenericRepository<Account> accountGenericRepository,
            IGenericRepository<Role> roleGenericRepository,
            IGenericRepository<PasswordResetToken> passwordResetTokenGenericRepository,
            IGenericRepository<AccountProfile> accountProfileGenericRepository,

            FilterTagService filterTagService,

            FluentEmailService fluentEmailService,

            DateHelper dateHelper,
            FileIOHelper fileIOHelper
            )
        {
            _logger = logger;
            _appDbContext = appDbContext;
            _bcryptHelper = bcryptHelper;
            _jwtHelper = jwtHelper;
            _unitOfWork = unitOfWork;
            _jwtConfig = jwtConfig;
            _appConfig = appConfig;
            _googleOAuth2Config = googleOAuth2Config;
            _filePathConfig = filePathConfig;
            _accountConfig = accountConfig;
            _googleMailConfig = googleMailConfig;

            _accountGenericRepository = accountGenericRepository;
            _roleGenericRepository = roleGenericRepository;
            _passwordResetTokenGenericRepository = passwordResetTokenGenericRepository;
            _accountProfileGenericRepository = accountProfileGenericRepository;

            _filterTagService = filterTagService;

            _fluentEmailService = fluentEmailService;

            _dateHelper = dateHelper;
            _fileIOHelper = fileIOHelper;
        }

        public async Task<JObject> LoginManual(ManualLoginDTO loginData)
        {
            var loginRequest = loginData.LoginInfo;
            //             Login với account chưa verify -> login thất bại (tài khoản không tồn tại)
            // Login với account đã verify -> login bình thường
            // Login với account không tồn tại -> login thất bại (tài khoản không tồn tại)

            var account = await _unitOfWork.AccountRepository.FindByEmailAsync(loginRequest.Email);
            if (account == null)
            {
                throw new HttpRequestException("Tài khoản không tồn tại");
            }
            if (account.DeactivatedAt != null && account.DeactivatedAt.Value < _dateHelper.GetNowByAppTimeZone())
            {
                throw new HttpRequestException("Tài khoản đã bị vô hiệu hoá");
            }

            if (account.IsVerified == false)
            {
                throw new HttpRequestException("Tài khoản chưa được xác thực");
            }

            if (!_bcryptHelper.VerifyPassword(loginRequest.Password, account.Password))
            {
                throw new HttpRequestException("Email hoặc mật khẩu không chính xác");
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
                    new Claim(ClaimTypes.SerialNumber, Guid.NewGuid().ToString()), // Mã định danh JWT
                };

                var token = _jwtHelper.GenerateJWT_TwoPublicPrivateKey(claims, _jwtConfig.Exp);

                var user = _jwtHelper.DecodeToken_TwoPublicPrivateKey(token);

                return JObject.FromObject(new
                {
                    Token = token,
                    // User = new
                    // {

                    //     Id = user.FindFirst("id")?.Value,
                    //     FullName = user.FindFirst(JwtRegisteredClaimNames.Name)?.Value,
                    //     Email = user.FindFirst(JwtRegisteredClaimNames.Email)?.Value,
                    //     Phone = user.FindFirst("phone")?.Value,
                    //     DayOfBirth = user.FindFirst("dayOfBirth")?.Value,
                    //     Address = user.FindFirst("address")?.Value,
                    //     Role = user.FindFirst(ClaimTypes.Role)?.Value

                    // }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine("\n" + ex.StackTrace + "\n");
                throw new HttpRequestException(ex.Message);
                // throw new HttpRequestException("Could not generate JWT");
            }


        }

        public async Task<JObject> LoginGoogleAuthorizationCodeFlow(GoogleLoginDTO googleLoginData)
        {
            string authorizationCode = googleLoginData.GoogleAuth.AuthorizationCode;
            string redirectUri = googleLoginData.GoogleAuth.RedirectUri;
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
                    var account = await _unitOfWork.AccountRepository.FindByEmailAsync(googlePayload.Email);
                    if (account == null)
                    {
                        // Chưa có account, tạo mới
                        var newAccount = new Account
                        {
                            Email = googlePayload.Email,
                            Password = _bcryptHelper.HashPassword(Guid.NewGuid().ToString()),
                            FullName = googlePayload.Name ?? googlePayload.Email,
                            RoleId = 4,
                            IsVerified = true,
                            GoogleId = googlePayload.Subject,
                            ProgressionSurveyCount = 0,
                            IsFilterSurveyRequired = true
                        };
                        newAccount = await _accountGenericRepository.CreateAsync(newAccount);
                        var AccountProfile = new AccountProfile
                        {
                            AccountId = newAccount.Id,
                            CountryRegion = null,
                            MaritalStatus = null,
                            AverageIncome = null,
                            EducationLevel = null,
                            JobField = null,
                            ProvinceCode = null,
                            DistrictCode = null,
                            WardCode = null
                        };
                        await _accountProfileGenericRepository.CreateAsync(AccountProfile);
                        await _filterTagService.RegisterFilterTag(newAccount.Id);

                        account = newAccount;
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

                                var newAccount = new Account
                                {
                                    Email = googlePayload.Email,
                                    Password = _bcryptHelper.HashPassword(Guid.NewGuid().ToString()),
                                    FullName = googlePayload.Name ?? googlePayload.Email,
                                    RoleId = 4,
                                    IsVerified = true,
                                    GoogleId = googlePayload.Subject,
                                    ProgressionSurveyCount = 0,
                                    IsFilterSurveyRequired = true
                                };
                                newAccount = await _accountGenericRepository.CreateAsync(newAccount);
                                var AccountProfile = new AccountProfile
                                {
                                    AccountId = newAccount.Id,
                                    CountryRegion = null,
                                    MaritalStatus = null,
                                    AverageIncome = null,
                                    EducationLevel = null,
                                    JobField = null,
                                    ProvinceCode = null,
                                    DistrictCode = null,
                                    WardCode = null
                                };
                                await _accountProfileGenericRepository.CreateAsync(AccountProfile);
                                await _filterTagService.RegisterFilterTag(newAccount.Id);
                                account = newAccount;
                            }
                        }
                        else if (account.GoogleId != googlePayload.Subject)
                        {
                            throw new HttpRequestException("Tài khoản đã liên kết với Google khác");
                        }
                        else if (account.GoogleId == googlePayload.Subject && !account.IsVerified)
                        {
                            throw new HttpRequestException("Tài khoản Google này chưa được kích hoạt");
                        }
                        // else: GoogleId trùng và đã verify => thành công
                    }

                    // Check deactivated
                    if (account.DeactivatedAt != null && account.DeactivatedAt.Value < _dateHelper.GetNowByAppTimeZone())
                    {
                        throw new HttpRequestException("Tài khoản đã bị vô hiệu hoá");
                    }

                    // Tạo claims và trả về giống LoginManual
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.NameIdentifier, account.Id.ToString()),
                        new Claim(ClaimTypes.Name, account.FullName ?? string.Empty),
                        new Claim(ClaimTypes.Email, account.Email),
                        new Claim("id", account.Id.ToString()),
                        new Claim("role_id", account.RoleId.ToString()),
                        new Claim(ClaimTypes.Role, account.Role?.Name ?? "User"),
                        new Claim("balance", account.Balance.ToString()),
                        new Claim(ClaimTypes.SerialNumber, Guid.NewGuid().ToString()),
                    };
                    var token = _jwtHelper.GenerateJWT_TwoPublicPrivateKey(claims, _jwtConfig.Exp);
                    await transaction.CommitAsync();
                    return JObject.FromObject(new { Token = token });
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    Console.WriteLine("\n" + ex.StackTrace + "\n");
                    // throw new HttpRequestException(ex.Message);
                    throw new UnauthorizedAccessException("Lỗi đăng nhập bằng Google");
                }
            }
        }

        public async Task AccountVerification(AccountVerificationDTO verificationData)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var account = await _unitOfWork.AccountRepository.FindByEmailAsync(verificationData.Email);
                    if (account == null)
                    {
                        throw new Exception("không tìm thấy tài khoản với email: " + verificationData.Email);
                    }
                    if (account.DeactivatedAt != null && account.DeactivatedAt.Value < _dateHelper.GetNowByAppTimeZone())
                    {
                        throw new HttpRequestException("Tài khoản đã bị vô hiệu hoá");
                    }
                    if (account.VerifyCode != verificationData.VerifyCode)
                    {
                        throw new HttpRequestException("Mã xác thực không chính xác");
                    }
                    account.IsVerified = true;
                    account.VerifyCode = null;

                    await _accountGenericRepository.UpdateAsync(account.Id, account);


                    var AccountProfile = new AccountProfile
                    {
                        AccountId = account.Id,
                        CountryRegion = null,
                        MaritalStatus = null,
                        AverageIncome = null,
                        EducationLevel = null,
                        JobField = null,
                        ProvinceCode = null,
                        DistrictCode = null,
                        WardCode = null
                    };
                    await _accountProfileGenericRepository.CreateAsync(AccountProfile);
                    await _filterTagService.RegisterFilterTag(account.Id);

                    await transaction.CommitAsync();
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    Console.WriteLine("\n" + ex.StackTrace + "\n");
                    throw new HttpRequestException("Xác thực tài khoản thất bại, lỗi: " + ex.Message);
                }
            }
        }



        public async Task ForgotPassword(string email)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var account = await _unitOfWork.AccountRepository.FindByEmailAsync(email);
                    if (account == null)
                    {
                        throw new HttpRequestException("Tài khoản không tồn tại");
                    }
                    if (account.DeactivatedAt != null && account.DeactivatedAt.Value < _dateHelper.GetNowByAppTimeZone())
                    {
                        throw new HttpRequestException("Tài khoản đã bị vô hiệu hoá");
                    }

                    string NewGuid = Guid.NewGuid().ToString();
                    await _unitOfWork.PasswordResetTokenRepository.CreateAsync(new PasswordResetToken
                    {
                        AccountId = account.Id,
                        Token = NewGuid,
                        ExpiredAt = _dateHelper.GetNowByAppTimeZone().AddHours(_appConfig.RESET_PASSWORD.TokenExpiredInMinutes),
                        IsUsed = false,
                        CreatedAt = _dateHelper.GetNowByAppTimeZone()
                    });
                    string resetPasswordUrl = _appConfig.RESET_PASSWORD.Url + "?email=" + email + "&token=" + NewGuid;
                    Console.WriteLine("Reset Password URL: " + resetPasswordUrl);

                    await _fluentEmailService.SendEmail(email, new ForgotPasswordEmailViewModel
                    {
                        Email = email,
                        PasswordResetToken = NewGuid,
                        ResetPasswordUrl = resetPasswordUrl,
                        ExpiredAt = _dateHelper.GetNowByAppTimeZone().AddHours(1).ToString("dd/MM/yyyy HH:mm:ss")
                    }, _googleMailConfig.AccountForgotPassword_TemplateViewPath
                    , _googleMailConfig.AccountForgotPassword_MailSubject);
                    await transaction.CommitAsync();
                }
                catch (HttpRequestException ex)
                {
                    await transaction.RollbackAsync();
                    Console.WriteLine("\n" + ex.StackTrace + "\n");
                    throw new HttpRequestException(ex.Message);
                }
            }

        }


        public async Task NewResetPassword(NewResetPasswordRequestDTO newResetPasswordRequest)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var account = await _unitOfWork.AccountRepository.FindByEmailAsync(newResetPasswordRequest.Email);
                    if (account == null)
                    {
                        throw new HttpRequestException("Tài khoản không tồn tại");
                    }
                    if (account.DeactivatedAt != null && account.DeactivatedAt.Value < _dateHelper.GetNowByAppTimeZone())
                    {
                        throw new HttpRequestException("Tài khoản đã bị vô hiệu hoá");
                    }

                    var passwordResetToken = await this._unitOfWork.PasswordResetTokenRepository.getValidToken(account.Id, newResetPasswordRequest.PasswordResetToken);

                    account.Password = _bcryptHelper.HashPassword(newResetPasswordRequest.NewPassword);
                    await _accountGenericRepository.UpdateAsync(account.Id, account);

                    passwordResetToken.IsUsed = true;
                    await _passwordResetTokenGenericRepository.UpdateAsync(passwordResetToken.Id, passwordResetToken);

                    await transaction.CommitAsync();
                }
                catch (HttpRequestException ex)
                {
                    await transaction.RollbackAsync();
                    Console.WriteLine("\n" + ex.StackTrace + "\n");
                    throw new HttpRequestException(ex.Message);
                }
            }
        }



    }
}
