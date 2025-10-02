using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Architecture_1.API.Filters.ExceptionFilters;
using Architecture_1.BusinessLogic.Helpers;
using Architecture_1.BusinessLogic.Services.DbServices.UserServices;
using Architecture_1.Common.AppConfigurations.FileStorage.interfaces;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;
using Google.Apis.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Architecture_1.Infrastructure.Services.AWS.S3;

namespace Architecture_1.API.Controllers.UserControllers
{
    [Route("api/User/auth")]
    [ApiController]
    [TypeFilter(typeof(HttpExceptionFilter))]
    public class AuthController : ControllerBase
    {
        private ILogger<AuthController> _logger;
        private readonly AuthService _authService;
        private readonly AccountService _accountService;
        private readonly AWSS3Service _awsS3Service;
        private readonly BcryptHelpers _bcryptHelpers;
        private readonly IFilePathConfig _filePathConfig;

        // Records
        public record LoginRequestBody(string Email, string Password);

        public AuthController(ILogger<AuthController> logger, AuthService authService, AccountService accountService,
            BcryptHelpers bcryptHelpers,
            AWSS3Service awsS3Service,
            IFilePathConfig filePathConfig
            )
        {
            _logger = logger;
            _authService = authService;
            _accountService = accountService;
            _bcryptHelpers = bcryptHelpers;
            _awsS3Service = awsS3Service;
            _filePathConfig = filePathConfig;
        }


        // [POST] /User/auth/login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] JToken jsonData)
        {
            LoginRequestBody loginRequestBody = jsonData["login"].ToObject<LoginRequestBody>();

            JObject login = await _authService.Login(loginRequestBody);



            return Ok(new
            {
                // message = "Đăng nhập thành công",
                message = "Login successfully",
                Token = login["Token"].ToString(),
                User = login["User"],
                //Data = login.Properties().ToDictionary(p => p.Name, p => p.Value.ToString()),

            });
        }


        [HttpPost("login-authorization-code-flow")]
        public async Task<IActionResult> LoginAuthorizationCodeFlow([FromBody] JToken jsonData)
        {
            dynamic login = jsonData["login"].ToObject<dynamic>();
            JObject result = await _authService.LoginAuthorizationCodeFlow(login);
            return Ok(new
            {
                message = "Login successfully",
                Token = result["Token"].ToString(),
                User = result["User"],
            });
        }



        /////////////////////////////////////////////////////////////////////////
        [HttpGet("verifyJWT/TwoPublicPrivateKey")]
        [Authorize(AuthenticationSchemes = "Jwt_PublicPrivateKeyAuth")]
        public IActionResult verifyJWT_TwoPublicPrivateKey()
        {
            return Ok(new { success = "Verify JWT TwoPublicPrivateKey successfully" });
        }


        [HttpPost("get-hash")]
        public IActionResult GetHash([FromBody] JToken jsonData)
        {
            try
            {
                var password = jsonData["password"].ToString();
                var hashedPassword = _bcryptHelpers.HashPassword(password);
                return Ok(hashedPassword);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }



        [HttpPost("dynamic-test")]
        public IActionResult DynamicTest([FromBody] JToken jsonData)
        {
            try
            {
                dynamic data = jsonData["data"].ToObject<dynamic>();
                dynamic data2 = new
                {
                    name = "data2 test",
                    age = 20
                };

                var data3 = new
                {
                    name = "data3 testtttt",
                    age = 20000
                };

                Console.WriteLine(data2.name + " " + data2.age);

                dynamic data4 = new ExpandoObject();
                data4.name = "data4 testtttt";
                data4.age = 20000;


                _authService.dynamicTest(data, data2, data3, data4);

                return Ok(data);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("demo-test")]
        public IActionResult DemoTest()
        {
            string[] strings = { "test1", null, "test3" };
            string str = null;
            string str2 = "test";
            string concatStrByDot = string.Join(". ", strings);

            return Ok(
                new
                {
                    message = "Demo test successfully",
                    str = str,
                    str_toString = str?.ToString(),
                    str2 = str2,
                    concatStrByDot = concatStrByDot,
                    strings = strings,
                }
            );
        }

        [HttpPost("S3-test")]
        public async Task<IActionResult> S3Test([FromBody] JToken jsonData)
        {
            try
            {
                var file = jsonData["file"].ToString();
                var fileName = jsonData["fileName"].ToString();
                var folder = jsonData["folder"].ToString();

                dynamic fileToGetUrl = jsonData["fileToGetUrl"].ToObject<dynamic>();

                var path = $"{folder}/{fileName}";

                // Console.WriteLine(file);
                //await _awsS3Service.UploadBase64FileAsync(file, path);


                // await _awsS3Service.CopyFileAsync(_filePathConfig.MAJOR_IMAGE_PATH + "/unknown.jpg", "nhap/ngu2");

                await _awsS3Service.DeleteFolderAsync("nhap/ngu");

                return Ok(new
                {
                    message = "S3 test successfully",
                    // url = await _awsS3Service.GeneratePresignedUrlAsync(_filePathConfig.MAJOR_IMAGE_PATH , fileToGetUrl.Id.ToString(), fileToGetUrl.Name.ToString(), 1),
                    filekey = await _awsS3Service.GetFullFileKeyAsync(_filePathConfig.MAJOR_IMAGE_PATH, "background"),
                });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("test-auth")]
        [Authorize(Policy = "AdminOnly")]
        public IActionResult TestAuth()
        {
            return Ok(new { message = "Test auth successfully" });
        }

    

    }




}

