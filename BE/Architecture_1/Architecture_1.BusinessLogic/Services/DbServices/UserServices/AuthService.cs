using Microsoft.Extensions.Logging;
using Architecture_1.BusinessLogic.Helpers;
using Architecture_1.DataAccess.Data;
using Architecture_1.DataAccess.Repositories.interfaces;
using Architecture_1.DataAccess.UOW;
using Architecture_1.DataAccess.Entities;
using System.Security.Claims;
using Architecture_1.Common.AppConfigurations.Jwt;
using System.Linq;


using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Identity.Data;
using Newtonsoft.Json.Linq;
using Architecture_1.Common.AppConfigurations.Jwt.interfaces;
using Architecture_1.Infrastructure.Services.AWS.S3;
using Microsoft.Extensions.Configuration;
using Architecture_1.Common.AppConfigurations.App.interfaces;
using Azure.Core;
using Google.Apis.Auth;
using Architecture_1.Infrastructure.Configurations.Google.interfaces;

namespace Architecture_1.BusinessLogic.Services.DbServices.UserServices
{
    public class AuthService
    {
        // LOGGER
        private readonly ILogger<AuthService> _logger;

        // CONFIG
        public readonly IAppConfig _appConfig;
        private readonly IJwtConfig _jwtConfig;
        private readonly IGoogleOAuth2Config _googleOAuth2Config;

        // DB CONTEXT
        private readonly AppDbContext _dbContext;

        // HELPERS
        private readonly BcryptHelpers _bcryptHelpers;
        private readonly JwtHelpers _jwtHelpers;

        // UNIT OF WORK
        private readonly IUnitOfWork _unitOfWork;

        // REPOSITORIES
        private readonly IGenericRepository<Account> _accountRepository;
        private readonly IGenericRepository<Role> _roleRepository;

        // AWS SERVICE
        private readonly AWSS3Service _awsS3Service;

        // RECORDS
        public record LoginRequestBody(string Email, string Password);


        public AuthService(ILogger<AuthService> logger, AppDbContext dbContext, BcryptHelpers bcryptHelpers, JwtHelpers jwtHelpers,
            IUnitOfWork unitOfWork,
            IGenericRepository<Account> accountRepository,
            IJwtConfig jwtConfig,
            AWSS3Service awsS3Service,
            IAppConfig appConfig,
            IGoogleOAuth2Config googleOAuth2Config,
            IGenericRepository<Role> roleRepository
            )
        {
            _logger = logger;
            _dbContext = dbContext;
            _bcryptHelpers = bcryptHelpers;
            _jwtHelpers = jwtHelpers;
            _unitOfWork = unitOfWork;
            _accountRepository = accountRepository;
            _jwtConfig = jwtConfig;
            _awsS3Service = awsS3Service;
            _appConfig = appConfig;
            _googleOAuth2Config = googleOAuth2Config;
            _roleRepository = roleRepository;
        }

        public async Task<JObject> Login(dynamic loginRequest)
        {
            var account = await _unitOfWork.AccountRepository.FindByEmail(loginRequest.Email);
            if (account == null)
            {
                // throw new HttpRequestException("Email or password is incorrect");
                throw new HttpRequestException("Account is not exist");
            }
            if (account.IsDeactivated == true)
            {
                throw new HttpRequestException("Account is deactivated");
            }

            if (!_bcryptHelpers.VerifyPassword(loginRequest.Password, account.Password))
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
                    new Claim("phone", account.Phone ?? string.Empty),
                    new Claim("dayOfBirth", account.DateOfBirth.ToString()),
                    new Claim("address", account.Address ?? string.Empty),
                    new Claim("role_id", account.Role.Id.ToString()),
                    new Claim(ClaimTypes.Role, account.Role.Name),
                    new Claim(ClaimTypes.SerialNumber, Guid.NewGuid().ToString()), // Mã định danh JWT
                };

                var token = _jwtHelpers.GenerateJWT_TwoPublicPrivateKey(claims, _jwtConfig.Exp);

                var user = _jwtHelpers.DecodeToken_TwoPublicPrivateKey(token);

                return JObject.FromObject(new
                {
                    Token = token,
                    User = new
                    {

                        Id = user.FindFirst("id")?.Value,
                        FullName = user.FindFirst(JwtRegisteredClaimNames.Name)?.Value,
                        Email = user.FindFirst(JwtRegisteredClaimNames.Email)?.Value,
                        Phone = user.FindFirst("phone")?.Value,
                        DayOfBirth = user.FindFirst("dayOfBirth")?.Value,
                        Address = user.FindFirst("address")?.Value,
                        Role = user.FindFirst(ClaimTypes.Role)?.Value

                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error generating JWT: " + ex.Message);
                throw new HttpRequestException(ex.Message);
                // throw new HttpRequestException("Could not generate JWT");
            }


        }

        

        public async Task<JObject> LoginAuthorizationCodeFlow(dynamic loginRequest)
        {
            string authorizationCode = loginRequest.authorizationCode;
            string redirectUri = loginRequest.redirectUri;
            var clientId = _googleOAuth2Config.ClientId;
            var clientSecret = _googleOAuth2Config.ClientSecret;


            // 1. Gửi request đổi authorization code lấy access_token và id_token
            using var httpClient = new HttpClient();
            var tokenRequest = new HttpRequestMessage(HttpMethod.Post, "https://oauth2.googleapis.com/token");

            Console.WriteLine("\nAuthorization Code: " + authorizationCode);
            Console.WriteLine("Redirect URI: " + redirectUri);
            Console.WriteLine("Client ID: " + clientId);
            Console.WriteLine("Client Secret: " + clientSecret);
            Console.WriteLine("\n");
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
            {
                throw new UnauthorizedAccessException("Error exchanging authorization code for tokens");
            }

            var payloadStr = await response.Content.ReadAsStringAsync();
            var payload = JObject.Parse(payloadStr);
            string idToken = payload["id_token"]?.ToString();

            if (string.IsNullOrEmpty(idToken))
            {
                throw new UnauthorizedAccessException("ID Token missing in response");
            }

            // 2. Xác thực id_token và trích xuất thông tin người dùng
            var googlePayload = await GoogleJsonWebSignature.ValidateAsync(idToken, new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = new[] { clientId }
            });

            if (googlePayload == null)
            {
                throw new UnauthorizedAccessException("Invalid ID Token");
            }

            // 3. Kiểm tra hoặc tạo người dùng mới
            using (var transaction = await _dbContext.Database.BeginTransactionAsync())
            {

                Account account = await _unitOfWork.AccountRepository.FindByEmail(googlePayload.Email);
                if (account != null && account.IsDeactivated == true)
                {
                    throw new HttpRequestException("Account is deactivated");
                }

                try
                {

                    try
                    {
                        if (account == null)
                        {
                            // Console.WriteLine("\n\n\nNULL : "+googlePayload.Name+" \n\n\n");

                            account = new Account
                            {
                                Email = googlePayload.Email.ToString(),
                                Password = _bcryptHelpers.HashPassword("123"),
                                FullName = string.IsNullOrEmpty(googlePayload.Name) ? googlePayload.Email.ToString() : googlePayload.Name.ToString(),
                                RoleId = 4,
                                JobTypeId = 1,
                                DateOfBirth = null,
                                Address = null,
                                Phone = null,
                            };

                            account = await _accountRepository.CreateAsync(account);
                            Console.WriteLine("Account created: " + account.ToString());
                        }


                        await transaction.CommitAsync();

                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        Console.WriteLine("\n\n\n" + ex.StackTrace + "\n\n\n");
                        throw new HttpRequestException("Failed to login: " + ex.Message);
                    }

                    Role role = await _roleRepository.FindByIdAsync(account.RoleId);

                    var claims = new List<Claim>
                        {
                            new Claim(JwtRegisteredClaimNames.Sub, account.Id.ToString()),
                            new Claim(JwtRegisteredClaimNames.Name, account.FullName),
                            new Claim(JwtRegisteredClaimNames.Email, account.Email),
                            new Claim("id", account.Id.ToString()),
                            new Claim("user_id", account.Id.ToString()),
                            new Claim("phone", account.Phone ?? string.Empty),
                            new Claim("dayOfBirth", account.DateOfBirth == null ? account.DateOfBirth.ToString() : string.Empty),
                            new Claim("address", account.Address ?? string.Empty),
                            new Claim("role_id", role.Id.ToString()),
                            new Claim("hahaha_user_id", "1"),
                            new Claim(ClaimTypes.Role, role.Name),
                            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()), // Mã định danh JWT
                        };

                    var token = _jwtHelpers.GenerateJWT_TwoPublicPrivateKey(claims, _jwtConfig.Exp);

                    var user = _jwtHelpers.DecodeToken_TwoPublicPrivateKey(token);

                    return JObject.FromObject(new
                    {
                        Token = token,
                        User = new
                        {
                            Id = user.FindFirst("id")?.Value,
                            FullName = user.FindFirst(JwtRegisteredClaimNames.Name)?.Value,
                            Email = user.FindFirst(JwtRegisteredClaimNames.Email)?.Value,
                            Phone = user.FindFirst("phone")?.Value,
                            DayOfBirth = user.FindFirst("dayOfBirth")?.Value,
                            Address = user.FindFirst("address")?.Value,
                            Role = user.FindFirst(ClaimTypes.Role)?.Value

                        }
                    });

                }
                catch (Exception ex)
                {
                    Console.WriteLine("\n\n\n" + ex.StackTrace + "\n\n\n");
                    Console.WriteLine("\n\n\n" + ex.ToString() + "\n\n\n");

                    throw new HttpRequestException("Failed to login: " + ex.Message);
                }
                

            }

        }


        /////////////////////////////////////////////////////////////
        public record DynamicTestRequestBody();
        public async void dynamicTest(dynamic jsonData, dynamic jsonData2, dynamic jsonData3, dynamic jsonData4)
        {
            string str1 = jsonData.name;
            int age = jsonData.age;
            Console.WriteLine(str1 + " " + age);

            // string str2 = jsonData2.name; // lỗi do jsonData2 không có thuộc tính name
            Console.WriteLine(jsonData2);  // ở đây in ra  { name = test, age = 20 }

            // string str3 = jsonData3.name; // lỗi do jsonData3 không có thuộc tính name
            // int age3 = jsonData3.age; // lỗi do jsonData3 không có thuộc tính age
            // Console.WriteLine(str3 + " " + age3);

            string str4 = jsonData4.name;
            int age4 = jsonData4.age;
            Console.WriteLine(str4 + " " + age4);


        }

    }
}
