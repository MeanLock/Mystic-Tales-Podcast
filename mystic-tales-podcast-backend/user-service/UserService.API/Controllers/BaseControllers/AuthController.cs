using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UserService.API.Filters.ExceptionFilters;
using UserService.BusinessLogic.DTOs.Auth;
using UserService.BusinessLogic.Helpers.FileHelpers;
using UserService.BusinessLogic.Models.CrossService;
using UserService.BusinessLogic.Services.CrossServiceServices.QueryServices;
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


        public AuthController(GenericQueryService genericQueryService, HttpServiceQueryClient httpServiceQueryClient, KafkaProducerService kafkaProducerService, IMessagingService messagingService, IFileValidationConfig fileValidationConfig, IFilePathConfig filePathConfig, FileIOHelper fileIOHelper)
        {
            _genericQueryService = genericQueryService;
            _httpServiceQueryClient = httpServiceQueryClient;
            _kafkaProducerService = kafkaProducerService;
            _messagingService = messagingService;
            _fileIOHelper = fileIOHelper;
            _fileValidationConfig = fileValidationConfig;
            _filePathConfig = filePathConfig;
        }

        ///api/user-service/api/auth/register/customer
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
                await _fileIOHelper.UploadBinaryFileAsync(fileBytes, _filePathConfig.ACCOUNT_TEMP_FILE_PATH, $"{Guid.NewGuid()}_{customerRegisterRequestDTO.MainImageFile.FileName}");
                mainImageFileKey = FilePathHelper.CombinePaths(_filePathConfig.ACCOUNT_TEMP_FILE_PATH, $"{Guid.NewGuid()}_{customerRegisterRequestDTO.MainImageFile.FileName}");

            }
            JObject requestData = JObject.FromObject(RegisterInfo);
            requestData["MainImageFileKey"] = mainImageFileKey;
            requestData["RoleId"] = 1;

            var startSagaTriggerMessage = _kafkaProducerService.PrepareStartSagaTriggerMessage("user-management-domain", requestData, null, "user-registration-flow");
            await _messagingService.SendSagaMessageAsync(startSagaTriggerMessage);
            return Ok(new
            {
                // customerRegisterRequestDTO = customerRegisterRequestDTO.RegisterInfo,
                // RegisterInfo = RegisterInfo,
                fileInfo = new
                {
                    fileName = customerRegisterRequestDTO.MainImageFile?.FileName,
                    fileSize = customerRegisterRequestDTO.MainImageFile?.Length,
                    contentType = customerRegisterRequestDTO.MainImageFile?.ContentType
                },
                requestData = requestData,
                startSagaTriggerMessage,
                SagaInstanceId = startSagaTriggerMessage.SagaInstanceId
            }
            );
        }



    }
}
