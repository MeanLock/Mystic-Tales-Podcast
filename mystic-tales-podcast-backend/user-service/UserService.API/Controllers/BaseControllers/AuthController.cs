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
        private readonly GenericQueryService _genericQueryService;
        private readonly HttpServiceQueryClient _httpServiceQueryClient;
        private readonly KafkaProducerService _kafkaProducerService;
        private readonly IMessagingService _messagingService;
        private readonly IFileValidationConfig _fileValidationConfig;
        private readonly IFilePathConfig _filePathConfig;
        private readonly FileIOHelper _fileIOHelper;
        private readonly AccountService _accountService;


        public AuthController(GenericQueryService genericQueryService, HttpServiceQueryClient httpServiceQueryClient, KafkaProducerService kafkaProducerService, IMessagingService messagingService, IFileValidationConfig fileValidationConfig, IFilePathConfig filePathConfig, FileIOHelper fileIOHelper, AccountService accountService)
        {
            _genericQueryService = genericQueryService;
            _httpServiceQueryClient = httpServiceQueryClient;
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
                byte[] fileBytes;
                using (var memoryStream = new MemoryStream())
                {
                    await customerRegisterRequestDTO.MainImageFile.CopyToAsync(memoryStream);
                    fileBytes = memoryStream.ToArray();
                }
                string newMainImageFileName = $"{Guid.NewGuid()}_{customerRegisterRequestDTO.MainImageFile.FileName}";
                await _fileIOHelper.UploadBinaryFileAsync(fileBytes, _filePathConfig.ACCOUNT_TEMP_FILE_PATH, newMainImageFileName);
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
        public async Task<IActionResult> RegisterStaff([FromBody] StaffRegisterRequestDTO staffRegisterRequestDTO)
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
                byte[] fileBytes;
                using (var memoryStream = new MemoryStream())
                {
                    await staffRegisterRequestDTO.MainImageFile.CopyToAsync(memoryStream);
                    fileBytes = memoryStream.ToArray();
                }
                string newMainImageFileName = $"{Guid.NewGuid()}_{staffRegisterRequestDTO.MainImageFile.FileName}";
                await _fileIOHelper.UploadBinaryFileAsync(fileBytes, _filePathConfig.ACCOUNT_TEMP_FILE_PATH, newMainImageFileName);
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



    }
}
