using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UserService.API.Filters.ExceptionFilters;
using UserService.BusinessLogic.DTOs.Auth;
using UserService.BusinessLogic.Helpers.FileHelpers;
using UserService.BusinessLogic.Models.CrossService;
using UserService.BusinessLogic.Services.CrossServiceServices.QueryServices;
using UserService.BusinessLogic.Services.DbServices.UserServices;
using UserService.BusinessLogic.Services.MessagingServices.interfaces;
using UserService.Common.AppConfigurations.BusinessSetting.interfaces;
using UserService.Common.AppConfigurations.FilePath.interfaces;
using UserService.Infrastructure.Services.Audio.AcoustID;
using UserService.Infrastructure.Services.Kafka;

namespace UserService.API.Controllers.BaseControllers
{
    [Route("api/auth")]
    [ApiController]
    [TypeFilter(typeof(HttpExceptionFilter))]
    public class AuthController : ControllerBase
    {
        private readonly KafkaProducerService _kafkaProducerService;
        private readonly IMessagingService _messagingService;
        private readonly IFileValidationConfig _fileValidationConfig;
        private readonly IFilePathConfig _filePathConfig;
        private readonly FileIOHelper _fileIOHelper;
        private readonly AccountService _accountService;


        public AuthController(KafkaProducerService kafkaProducerService, IMessagingService messagingService, IFileValidationConfig fileValidationConfig, IFilePathConfig filePathConfig, FileIOHelper fileIOHelper, AccountService accountService)
        {
            _kafkaProducerService = kafkaProducerService;
            _messagingService = messagingService;
            _fileIOHelper = fileIOHelper;
            _fileValidationConfig = fileValidationConfig;
            _filePathConfig = filePathConfig;
            _accountService = accountService;
        }

        [HttpGet("test")]
        public async Task<IActionResult> Test()
        {
            return Ok(await _accountService.GetActiveSystemConfigProfile());
        }

        // /api/user-service/api/auth/register/customer
        [HttpPost("register/customer")]
        public async Task<IActionResult> RegisterCustomer([FromForm] CustomerRegisterRequestDTO customerRegisterRequestDTO)
        {
            var RegisterInfo = JsonConvert.DeserializeObject<CustomerRegisterInfoDTO>(customerRegisterRequestDTO.RegisterInfo);

            string mainImageFileKey = null;
            if (customerRegisterRequestDTO.MainImageFile != null)
            {
                // bool IsValidFile(string fieldName, string fileName, long fileSizeBytes, string mimeType);
                var isValidImage = _fileValidationConfig.IsValidFile("Account.mainImageFileKey", customerRegisterRequestDTO.MainImageFile.FileName, customerRegisterRequestDTO.MainImageFile.Length, customerRegisterRequestDTO.MainImageFile.ContentType);
                if (!isValidImage)
                {
                    return BadRequest("Invalid image file.");
                }


                string newMainImageFileName = $"{Guid.NewGuid()}_{customerRegisterRequestDTO.MainImageFile.FileName}";
                using (var stream = customerRegisterRequestDTO.MainImageFile.OpenReadStream())
                {
                    await _fileIOHelper.UploadBinaryFileWithStreamAsync(
                                        stream,
                                        _filePathConfig.ACCOUNT_TEMP_FILE_PATH,
                                        newMainImageFileName
                                    );
                }

                mainImageFileKey = FilePathHelper.CombinePaths(_filePathConfig.ACCOUNT_TEMP_FILE_PATH, newMainImageFileName);

            }
            JObject requestData = JObject.FromObject(RegisterInfo);
            requestData["MainImageFileKey"] = mainImageFileKey;
            requestData["RoleId"] = 1;

            var startSagaTriggerMessage = _kafkaProducerService.PrepareStartSagaTriggerMessage("user-management-domain", requestData, null, "user-registration-flow");
            await _messagingService.SendSagaMessageAsync(startSagaTriggerMessage);
            return Ok(new
            {
                SagaInstanceId = startSagaTriggerMessage.SagaInstanceId
            }
            );
        }


        // /api/user-service/api/auth/account-verification
        [HttpPost("account-verification")]
        public async Task<IActionResult> AccountVerification([FromBody] AccountVerificationRequestDTO accountVerificationRequestDTO)
        {
            // var accountVerificationInfo = JsonConvert.DeserializeObject<AccountVerificationInfoDTO>(accountVerificationRequestDTO.AccountVerificationInfo);
            var requestData = JObject.FromObject(accountVerificationRequestDTO.AccountVerificationInfo);
            var startSagaTriggerMessage = _kafkaProducerService.PrepareStartSagaTriggerMessage("user-management-domain", requestData, null, "user-email-verification-flow");
            await _messagingService.SendSagaMessageAsync(startSagaTriggerMessage);
            return Ok(new
            {
                SagaInstanceId = startSagaTriggerMessage.SagaInstanceId
            }
            );
        }

        // /api/user-service/api/auth/register/staff
        [HttpPost("register/staff")]
        public async Task<IActionResult> RegisterStaff([FromForm] StaffRegisterRequestDTO staffRegisterRequestDTO)
        {
            var RegisterInfo = JsonConvert.DeserializeObject<StaffRegisterInfoDTO>(staffRegisterRequestDTO.RegisterInfo);

            string mainImageFileKey = null;
            if (staffRegisterRequestDTO.MainImageFile != null)
            {
                // bool IsValidFile(string fieldName, string fileName, long fileSizeBytes, string mimeType);
                var isValidImage = _fileValidationConfig.IsValidFile("Account.mainImageFileKey", staffRegisterRequestDTO.MainImageFile.FileName, staffRegisterRequestDTO.MainImageFile.Length, staffRegisterRequestDTO.MainImageFile.ContentType);
                if (!isValidImage)
                {
                    return BadRequest("Invalid image file.");
                }

                string newMainImageFileName = $"{Guid.NewGuid()}_{staffRegisterRequestDTO.MainImageFile.FileName}";
                using (var stream = staffRegisterRequestDTO.MainImageFile.OpenReadStream())
                {
                    await _fileIOHelper.UploadBinaryFileWithStreamAsync(
                                        stream,
                                        _filePathConfig.ACCOUNT_TEMP_FILE_PATH,
                                        newMainImageFileName
                                    );
                }
                mainImageFileKey = FilePathHelper.CombinePaths(_filePathConfig.ACCOUNT_TEMP_FILE_PATH, newMainImageFileName);

            }
            JObject requestData = JObject.FromObject(RegisterInfo);
            requestData["MainImageFileKey"] = mainImageFileKey;
            requestData["RoleId"] = 2;

            var startSagaTriggerMessage = _kafkaProducerService.PrepareStartSagaTriggerMessage("user-management-domain", requestData, null, "user-registration-flow");
            await _messagingService.SendSagaMessageAsync(startSagaTriggerMessage);
            return Ok(new
            {
                SagaInstanceId = startSagaTriggerMessage.SagaInstanceId
            }
            );
        }


        // /api/user-service/api/auth/login-manual
        [HttpPost("login-manual")]
        public async Task<IActionResult> LoginManual([FromBody] ManualLoginRequestDTO manualLoginRequestDTO)
        {
            var requestData = JObject.FromObject(manualLoginRequestDTO.ManualLoginInfo);
            var startSagaTriggerMessage = _kafkaProducerService.PrepareStartSagaTriggerMessage("user-management-domain", requestData, null, "user-manual-login-flow");
            await _messagingService.SendSagaMessageAsync(startSagaTriggerMessage);
            return Ok(new
            {
                SagaInstanceId = startSagaTriggerMessage.SagaInstanceId
            }
            );
        }

        // /api/user-service/api/auth/login-google
        [HttpPost("login-google")]
        public async Task<IActionResult> LoginGoogle([FromBody] GoogleLoginRequestDTO googleLoginRequestDTO)
        {
            var requestData = JObject.FromObject(googleLoginRequestDTO.GoogleAuth);
            var startSagaTriggerMessage = _kafkaProducerService.PrepareStartSagaTriggerMessage("user-management-domain", requestData, null, "user-google-login-flow");
            await _messagingService.SendSagaMessageAsync(startSagaTriggerMessage);
            return Ok(new
            {
                SagaInstanceId = startSagaTriggerMessage.SagaInstanceId
            }
            );
        }

        // /api/user-service/api/auth/forgot-password
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestDTO forgotPasswordRequestDTO)
        {
            var requestData = JObject.FromObject(forgotPasswordRequestDTO);
            var startSagaTriggerMessage = _kafkaProducerService.PrepareStartSagaTriggerMessage("user-management-domain", requestData, null, "forgot-password-flow");
            await _messagingService.SendSagaMessageAsync(startSagaTriggerMessage);
            return Ok(new
            {
                SagaInstanceId = startSagaTriggerMessage.SagaInstanceId
            }
            );
        }

        // /api/user-service/api/auth/new-reset-password
        [HttpPost("new-reset-password")]
        public async Task<IActionResult> NewResetPassword([FromBody] NewResetPasswordRequestDTO newResetPasswordRequestDTO)
        {
            var requestData = JObject.FromObject(newResetPasswordRequestDTO.ResetPasswordInfo);
            var startSagaTriggerMessage = _kafkaProducerService.PrepareStartSagaTriggerMessage("user-management-domain", requestData, null, "password-reset-flow");
            await _messagingService.SendSagaMessageAsync(startSagaTriggerMessage);
            return Ok(new
            {
                SagaInstanceId = startSagaTriggerMessage.SagaInstanceId
            }
            );
        }



    }
}
