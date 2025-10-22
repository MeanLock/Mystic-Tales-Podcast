using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SurveyTalkService.API.Filters.ExceptionFilters;
using SurveyTalkService.Common.AppConfigurations.FilePath.interfaces;
using SurveyTalkService.BusinessLogic.Services.DbServices.UserServices;
using Newtonsoft.Json.Linq;
using SurveyTalkService.Common.AppConfigurations.App.interfaces;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;
using SurveyTalkService.BusinessLogic.DTOs.Auth;
using SurveyTalkService.DataAccess.Data;
using Microsoft.EntityFrameworkCore;
using Pgvector;
using SurveyTalkService.BusinessLogic.Helpers.AuthHelpers;
using SurveyTalkService.BusinessLogic.Helpers.DateHelpers;


namespace SurveyTalkService.API.Controllers.UserControllers
{
    [Route("api/User/auth")]
    [ApiController]
    [TypeFilter(typeof(HttpExceptionFilter))]
    public class AuthController : ControllerBase
    {
        private ILogger<AuthController> _logger;
        private readonly AuthService _authService;
        private readonly AccountService _accountService;
        private readonly BcryptHelper _bcryptHelper;
        private readonly IFilePathConfig _filePathConfig;
        private readonly DateHelper _dateHelper;

        // Configs
        private readonly IAppConfig _appConfig;

        // Records
        public record LoginRequestBody(string Email, string Password);

        private readonly PostgresDbContext _postgresDbContext;

        private readonly IHttpClientFactory _httpClientFactory;

        public AuthController(
            ILogger<AuthController> logger,
            AuthService authService,
            AccountService accountService,
            BcryptHelper bcryptHelper,
            IFilePathConfig filePathConfig,
            DateHelper dateHelper,
            IAppConfig appConfig,
            PostgresDbContext postgresDbContext,
            IHttpClientFactory httpClientFactory
            )
        {
            _logger = logger;
            _authService = authService;
            _accountService = accountService;
            _bcryptHelper = bcryptHelper;
            _filePathConfig = filePathConfig;
            _dateHelper = dateHelper;
            _appConfig = appConfig;
            _postgresDbContext = postgresDbContext;
            _httpClientFactory = httpClientFactory;
        }

        // POST /api/User/auth/login-manual
        [HttpPost("login-manual")]
        public async Task<IActionResult> Login([FromBody] ManualLoginDTO loginData)
        {
            JObject login = await _authService.LoginManual(loginData);

            return Ok(new
            {
                Message = "Đăng nhập thành công",
                AccessToken = login["Token"].ToString(),
                // Message = "Login successfully",
                // User = login["User"],
                // Data = login.Properties().ToDictionary(p => p.Name, p => p.Value.ToString()),
            });
        }

        // POST /api/User/auth/login-google
        [HttpPost("login-google")]
        public async Task<IActionResult> LoginGoogle([FromBody] GoogleLoginDTO googleLoginData)
        {
            JObject login = await _authService.LoginGoogleAuthorizationCodeFlow(googleLoginData);

            return Ok(new
            {
                Message = "Đăng nhập bằng Google thành công",
                AccessToken = login["Token"].ToString(),
                // User = login["User"],
                // Data = login.Properties().ToDictionary(p => p.Name, p => p.Value.ToString()),
            });
        }

        // POST /api/User/auth/register/customer
        [HttpPost("register/customer")]
        public async Task<IActionResult> RegisterCustomer([FromBody] CustomerRegisterDTO registerData)
        {
            await _accountService.RegisterCustomer(registerData);

            return Ok(new
            {
                Message = "Đăng ký tài khoản thành công",
            });
        }

        // POST /api/User/auth/account-verification
        [HttpPost("account-verification")]
        public async Task<IActionResult> AccountVerification([FromBody] AccountVerificationDTO verificationData)
        {
            await _authService.AccountVerification(verificationData);

            return Ok(new
            {
                Message = "Xác thực tài khoản thành công",
            });
        }

        // POST /api/User/auth/register/staff
        [HttpPost("register/staff")]
        [Authorize(Policy = "AdminRequiredOnly")]
        public async Task<IActionResult> RegisterStaff([FromBody] StaffRegisterDTO registerData)
        {
            Console.WriteLine("Registering staff with data: " + registerData.RegisterInfo.RoleId);
            await _accountService.RegisterStaff(registerData);

            return Ok(new
            {
                Message = "Đăng ký tài khoản nhân viên thành công",
            });
        }

        // POST /api/User/auth/forgot-password
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] JToken data)
        {
            string email = data["Email"].ToString();
            if (string.IsNullOrEmpty(email))
            {
                return BadRequest(new { Message = "Email không được để trống" });
            }

            await _authService.ForgotPassword(email);


            return Ok(new { Message = "Yêu cầu đặt lại mật khẩu đã được gửi đến email của bạn. Vui lòng kiểm tra email của bạn để truy cập lại mật khẩu" });
        }

        // GET /api/User/auth/new-reset-password
        [HttpPost("new-reset-password")]
        public async Task<IActionResult> NewResetPassword([FromBody] NewResetPasswordRequestDTO newResetPasswordRequest)
        {
            await _authService.NewResetPassword(newResetPasswordRequest);

            return Ok(new
            {
                Message = "Cập nhật mật khẩu mới thành công. Vui lòng đăng nhập lại",
            });
        }

        // 

        ///////////////////////////////////////////////////////////////

        [HttpGet("get-date")]
        public async Task<IActionResult> GetDate()
        {
            var currentDate = DateTime.UtcNow;
            // await _authService.testDeleteFile();
            // await _authService.testDeleteFolder();
            var result = await _postgresDbContext.TakerEmbeddingVectorTagFilters.Where(t => t.TakerId == 6).ToListAsync();

            return Ok(new
            {
                ByTimeZoneId = _dateHelper.GetNowByAppTimeZone(),
                abc =new
                {
                    result = result.Select(r => new
                    {
                        r.TakerId,
                        r.FilterTagId,
                        EmbeddingVector = r.EmbeddingVector != null ? r.EmbeddingVector.ToArray() : null
                    })
                }
            });
        }


        [HttpPost("test-postgre")]
        public async Task<IActionResult> TestPostgre([FromBody] JToken data)
        {
            // Giả sử bạn muốn lấy summary từ data
            string summary = data["summary"].ToString();

            var client = _httpClientFactory.CreateClient();
            var response = await client.PostAsJsonAsync(
                "http://127.0.0.1:8000/paraphrase_multilingual_MiniLM_L12_v2/Vietnamese_document_embedding_model-vector-encode",
                new { summary }
            );
            response.EnsureSuccessStatusCode();
            var apiResult = JObject.Parse(await response.Content.ReadAsStringAsync()).ToObject<VectorResponse>();
            string json = await response.Content.ReadAsStringAsync();
            float[] vector = apiResult.vector; // giả sử trả về { vector: [...] }
            // return Ok(new {  vector });

            // Update row có takerId = 1
            var row = await _postgresDbContext.TakerEmbeddingVectorTagFilters.FirstOrDefaultAsync(t => t.TakerId == 1);

            row.EmbeddingVector = new Vector(vector);
            await _postgresDbContext.SaveChangesAsync();

            // Trả về row mới
            var result = await _postgresDbContext.TakerEmbeddingVectorTagFilters.Where(t => t.TakerId == 1).ToListAsync();
            return Ok(new
            {
                ok = new
                {
                    result = result.Select(r => new
                    {
                        r.TakerId,
                        r.FilterTagId,
                        EmbeddingVector = r.EmbeddingVector != null ? r.EmbeddingVector.ToArray() : null
                    })
                }
            });


        }
    }
    public class VectorResponse
    {
        public float[] vector { get; set; }
    }



}

