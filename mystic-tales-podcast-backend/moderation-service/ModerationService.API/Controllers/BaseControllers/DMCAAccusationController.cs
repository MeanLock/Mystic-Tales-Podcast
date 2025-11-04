using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ModerationService.API.Filters.ExceptionFilters;
using ModerationService.BusinessLogic.DTOs.Cache;
using ModerationService.BusinessLogic.DTOs.CounterNotice;
using ModerationService.BusinessLogic.DTOs.DMCANotice;
using ModerationService.BusinessLogic.DTOs.DMCAReport;
using ModerationService.BusinessLogic.DTOs.LawsuitProof;
using ModerationService.BusinessLogic.Enums.DMCA;
using ModerationService.BusinessLogic.Enums.Kafka;
using ModerationService.BusinessLogic.Helpers.FileHelpers;
using ModerationService.BusinessLogic.Models.CrossService;
using ModerationService.BusinessLogic.Services.CrossServiceServices.QueryServices;
using ModerationService.BusinessLogic.Services.DbServices.DMCAServices;
using ModerationService.BusinessLogic.Services.MessagingServices.interfaces;
using ModerationService.Common.AppConfigurations.BusinessSetting.interfaces;
using ModerationService.Common.AppConfigurations.FilePath.interfaces;
using ModerationService.DataAccess.Entities.SqlServer;
using ModerationService.Infrastructure.Models.Audio.AcoustID;
using ModerationService.Infrastructure.Services.Kafka;
using Newtonsoft.Json.Linq;

namespace ModerationService.API.Controllers.BaseControllers
{
    [Route("api/dmca-accusations")]
    [ApiController]
    [TypeFilter(typeof(HttpExceptionFilter))]
    [Authorize(Policy = "OptionalAccess")]
    public class DMCAAccusationController : ControllerBase
    {
        private readonly GenericQueryService _genericQueryService;
        private readonly HttpServiceQueryClient _httpServiceQueryClient;

        private readonly DMCAAccusationService _dmcaAccusationService;
        private readonly DMCANoticeService _dmcaNoticeService;
        private readonly CounterNoticeService _counterNoticeService;
        private readonly LawsuitProofService _lawsuitProofService;

        private readonly KafkaProducerService _kafkaProducerService;
        private readonly IMessagingService _messagingService;
        private readonly IFileValidationConfig _fileValidationConfig;
        private readonly IFilePathConfig _filePathConfig;
        private readonly FileIOHelper _fileIOHelper;
        private const string SAGA_TOPIC = KafkaTopicEnum.DmcaManagementDomain;

        private readonly ILogger<DMCAAccusationController> _logger;
        public DMCAAccusationController(
            GenericQueryService genericQueryService,
            HttpServiceQueryClient httpServiceQueryClient,
            DMCAAccusationService dmcaAccusationService,
            DMCANoticeService dmcaNoticeService,
            CounterNoticeService counterNoticeService,
            LawsuitProofService lawsuitProofService,
            KafkaProducerService kafkaProducerService,
            IMessagingService messagingService,
            IFileValidationConfig fileValidationConfig,
            IFilePathConfig filePathConfig,
            FileIOHelper fileIOHelper,
            ILogger<DMCAAccusationController> logger)
        {
            _genericQueryService = genericQueryService;
            _httpServiceQueryClient = httpServiceQueryClient;
            _dmcaAccusationService = dmcaAccusationService;
            _dmcaNoticeService = dmcaNoticeService;
            _counterNoticeService = counterNoticeService;
            _lawsuitProofService = lawsuitProofService;
            _kafkaProducerService = kafkaProducerService;
            _messagingService = messagingService;
            _fileValidationConfig = fileValidationConfig;
            _filePathConfig = filePathConfig;
            _fileIOHelper = fileIOHelper;
            _logger = logger;
        }
        [HttpGet]
        [Authorize(Policy = "AdminOrStaff.BasicAccess")]
        public async Task<IActionResult> GetDMCAAccusations()
        {
            var account = HttpContext.Items["LoggedInAccount"] as AccountStatusCache;
            var accountId = account.Id;
            var roleId = account.RoleId;

            var dmcaAccusations = await _dmcaAccusationService.GetAllDMCAAccusationForStaffOrAdminAsync(accountId, roleId);
            return Ok(new
            {
                DMCAAccusationList = dmcaAccusations
            });
        }
        [HttpPost("shows/{PodcastShowId}")]
        [Authorize(Policy = "Admin.BasicAccess")]
        public async Task<IActionResult> CreateDMCAAccusationForShow(
            [FromRoute] Guid PodcastShowId,
            [FromBody] DMCANoticeCreateRequestDTO request)
        {
            var account = HttpContext.Items["LoggedInAccount"] as AccountStatusCache;
            var loginAccountId = account.Id;

            // Validate all attach files first
            foreach (var attachFile in request.DMCANoticeAttachFiles)
            {
                var isValidDocumentFile = _fileValidationConfig.IsValidFile("DMCANoticeAttachFile.attachFileKey", attachFile.FileName, attachFile.Length, attachFile.ContentType);
                if (!isValidDocumentFile)
                {
                    return BadRequest($"Invalid document file '{attachFile.FileName}'. Please ensure all document files have correct type and size.");
                }
            }

            var attachFileList = new List<string>();

            // Process all attach files and prepare attach file list items
            foreach (var attachFile in request.DMCANoticeAttachFiles)
            {
                try
                {
                    string newAttachFileName = $"{Guid.NewGuid()}_{attachFile.FileName}";
                    Console.WriteLine($"Generated new attach file name: {newAttachFileName}");

                    using (var memoryStream = attachFile.OpenReadStream())
                    {
                        await _fileIOHelper.UploadBinaryFileWithStreamAsync(memoryStream, _filePathConfig.DMCA_ACCUSATION_TEMP_FILE_PATH, newAttachFileName);
                    }

                    var attachFileKey = FilePathHelper.CombinePaths(_filePathConfig.DMCA_ACCUSATION_TEMP_FILE_PATH, newAttachFileName);
                    attachFileList.Add(attachFileKey);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to process file {FileName}", attachFile.FileName);
                    return StatusCode(500, $"Failed to process file '{attachFile.FileName}'.");
                }
            }

            var requestData = new JObject
            {
                { "AccuserEmail", request.AccuserEmail },
                { "AccuserPhone", request.AccuserPhone },
                { "AccuserFullName", request.AccuserFullName },
                { "PodcastShowId", PodcastShowId },
                { "DMCANoticeAttachFileKeys", JArray.FromObject(attachFileList) }
            };
            var startSagaTriggerMessage = _kafkaProducerService.PrepareStartSagaTriggerMessage(
                topic: SAGA_TOPIC,
                requestData: requestData,
                sagaInstanceId: null,
                messageName: "content-dmca-accusation-creation-flow");
            var result = await _messagingService.SendSagaMessageAsync(startSagaTriggerMessage);
            if (!result)
            {
                return StatusCode(500, "Failed to initiate dmca accusation creation.");
            }
            return Ok(new
            {
                SagaInstanceId = startSagaTriggerMessage.SagaInstanceId
            });
        }
        [HttpPost("episodes/{PodcastEpisodeId}")]
        [Authorize(Policy = "Admin.BasicAccess")]
        public async Task<IActionResult> CreateDMCAAccusationForEpisode(
            [FromRoute] Guid PodcastEpisodeId,
            [FromBody] DMCANoticeCreateRequestDTO request)
        {
            var account = HttpContext.Items["LoggedInAccount"] as AccountStatusCache;
            var loginAccountId = account.Id;

            // Validate all attach files first
            foreach (var attachFile in request.DMCANoticeAttachFiles)
            {
                var isValidDocumentFile = _fileValidationConfig.IsValidFile("DMCANoticeAttachFile.attachFileKey", attachFile.FileName, attachFile.Length, attachFile.ContentType);
                if (!isValidDocumentFile)
                {
                    return BadRequest($"Invalid document file '{attachFile.FileName}'. Please ensure all document files have correct type and size.");
                }
            }

            var attachFileList = new List<string>();

            // Process all attach files and prepare attach file list items
            foreach (var attachFile in request.DMCANoticeAttachFiles)
            {
                try
                {
                    string newAttachFileName = $"{Guid.NewGuid()}_{attachFile.FileName}";
                    Console.WriteLine($"Generated new attach file name: {newAttachFileName}");

                    using (var memoryStream = attachFile.OpenReadStream())
                    {
                        await _fileIOHelper.UploadBinaryFileWithStreamAsync(memoryStream, _filePathConfig.DMCA_ACCUSATION_TEMP_FILE_PATH, newAttachFileName);
                    }

                    var attachFileKey = FilePathHelper.CombinePaths(_filePathConfig.DMCA_ACCUSATION_TEMP_FILE_PATH, newAttachFileName);
                    attachFileList.Add(attachFileKey);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to process file {FileName}", attachFile.FileName);
                    return StatusCode(500, $"Failed to process file '{attachFile.FileName}'.");
                }
            }

            var requestData = new JObject
            {
                { "AccuserEmail", request.AccuserEmail },
                { "AccuserPhone", request.AccuserPhone },
                { "AccuserFullName", request.AccuserFullName },
                { "PodcastEpisodeId", PodcastEpisodeId },
                { "DMCANoticeAttachFileKeys", JArray.FromObject(attachFileList) }
            };
            var startSagaTriggerMessage = _kafkaProducerService.PrepareStartSagaTriggerMessage(
                topic: SAGA_TOPIC,
                requestData: requestData,
                sagaInstanceId: null,
                messageName: "content-dmca-accusation-creation-flow");
            var result = await _messagingService.SendSagaMessageAsync(startSagaTriggerMessage);
            if (!result)
            {
                return StatusCode(500, "Failed to initiate dmca accusation creation.");
            }
            return Ok(new
            {
                SagaInstanceId = startSagaTriggerMessage.SagaInstanceId
            });
        }
        [HttpGet("{DMCAAccusationId}")]
        [Authorize(Policy = "Customer.BasicAccess")]
        public async Task<IActionResult> GetDMCAAccusationById(
            [FromRoute] int DMCAAccusationId)
        {
            var account = HttpContext.Items["LoggedInAccount"] as AccountStatusCache;
            var accountId = account.Id;
            var roleId = account.RoleId;

            var dmcaAccusation = await _dmcaAccusationService.GetDMCAAccusationByIdForStaffOrAdminAsync(DMCAAccusationId, accountId, roleId);
            return Ok(new
            {
                DMCAAccusation = dmcaAccusation
            });
        }
        [HttpPost("{DMCAAccusationId}/counter-notice")]
        [Authorize(Policy = "Admin.BasicAccess")]
        public async Task<IActionResult> CreateCounterNotice(
            [FromRoute] int DMCAAccusationId,
            [FromBody] CounterNoticeCreateRequestDTO request)
        {
            var account = HttpContext.Items["LoggedInAccount"] as AccountStatusCache;
            var loginAccountId = account.Id;

            // Validate all attach files first
            foreach (var attachFile in request.CounterNoticeAttachFiles)
            {
                var isValidDocumentFile = _fileValidationConfig.IsValidFile("CounterNoticeAttachFile.attachFileKey", attachFile.FileName, attachFile.Length, attachFile.ContentType);
                if (!isValidDocumentFile)
                {
                    return BadRequest($"Invalid document file '{attachFile.FileName}'. Please ensure all document files have correct type and size.");
                }
            }

            var attachFileList = new List<string>();

            // Process all attach files and prepare attach file list items
            foreach (var attachFile in request.CounterNoticeAttachFiles)
            {
                try
                {
                    string newAttachFileName = $"{Guid.NewGuid()}_{attachFile.FileName}";
                    Console.WriteLine($"Generated new attach file name: {newAttachFileName}");

                    using (var memoryStream = attachFile.OpenReadStream())
                    {
                        await _fileIOHelper.UploadBinaryFileWithStreamAsync(memoryStream, _filePathConfig.DMCA_ACCUSATION_TEMP_FILE_PATH, newAttachFileName);
                    }

                    var attachFileKey = FilePathHelper.CombinePaths(_filePathConfig.DMCA_ACCUSATION_TEMP_FILE_PATH, newAttachFileName);
                    attachFileList.Add(attachFileKey);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to process file {FileName}", attachFile.FileName);
                    return StatusCode(500, $"Failed to process file '{attachFile.FileName}'.");
                }
            }

            var requestData = new JObject
            {
                { "DMCAAccusationId", DMCAAccusationId },
                { "AccountId", request.CounterNoticeCreateInfo.AccountId },
                { "AccountEmail", request.CounterNoticeCreateInfo.AccountEmail },
                { "AccountPhone", request.CounterNoticeCreateInfo.AccountPhone },
                { "StatementPerjury", request.CounterNoticeCreateInfo.StatementPerjury },
                { "Jurisdiction", request.CounterNoticeCreateInfo.Juridisction },
                { "Signature", request.CounterNoticeCreateInfo.Signature },
                { "FiledDate", request.CounterNoticeCreateInfo.FiledDate },
                { "CounterNoticeAttachFileKeys", JArray.FromObject(attachFileList) }
            };
            var startSagaTriggerMessage = _kafkaProducerService.PrepareStartSagaTriggerMessage(
                topic: "dmca-management-domain",
                requestData: requestData,
                sagaInstanceId: null,
                messageName: "content-dmca-counter-notice-creation-flow");
            var result = await _messagingService.SendSagaMessageAsync(startSagaTriggerMessage);
            if (!result)
            {
                return StatusCode(500, "Failed to initiate dmca counter notice creation.");
            }
            return Ok(new
            {
                SagaInstanceId = startSagaTriggerMessage.SagaInstanceId
            });
        }
        [HttpPost("{DMCAAccusationId}/lawsuit")]
        [Authorize(Policy = "Admin.BasicAccess")]
        public async Task<IActionResult> SubmitLawsuitProof(
            [FromRoute] int DMCAAccusationId,
            [FromBody] LawsuitProofSubmitRequestDTO request)
        {
            var account = HttpContext.Items["LoggedInAccount"] as AccountStatusCache;
            var loginAccountId = account.Id;

            // Validate all attach files first
            foreach (var attachFile in request.LawsuitProofAttachFileKeys)
            {
                var isValidDocumentFile = _fileValidationConfig.IsValidFile("LawsuitProofAttachFile.attachFileKey", attachFile.FileName, attachFile.Length, attachFile.ContentType);
                if (!isValidDocumentFile)
                {
                    return BadRequest($"Invalid document file '{attachFile.FileName}'. Please ensure all document files have correct type and size.");
                }
            }

            var attachFileList = new List<string>();

            // Process all attach files and prepare attach file list items
            foreach (var attachFile in request.LawsuitProofAttachFileKeys)
            {
                try
                {
                    string newAttachFileName = $"{Guid.NewGuid()}_{attachFile.FileName}";
                    Console.WriteLine($"Generated new attach file name: {newAttachFileName}");

                    using (var memoryStream = attachFile.OpenReadStream())
                    {
                        await _fileIOHelper.UploadBinaryFileWithStreamAsync(memoryStream, _filePathConfig.DMCA_ACCUSATION_TEMP_FILE_PATH, newAttachFileName);
                    }

                    var attachFileKey = FilePathHelper.CombinePaths(_filePathConfig.DMCA_ACCUSATION_TEMP_FILE_PATH, newAttachFileName);
                    attachFileList.Add(attachFileKey);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to process file {FileName}", attachFile.FileName);
                    return StatusCode(500, $"Failed to process file '{attachFile.FileName}'.");
                }
            }

            var requestData = new JObject
            {
                { "DMCAAccusationId", DMCAAccusationId },
                { "AccountId", request.LawsuitProofCreateInfo.AccountId },
                { "GoodFaithStatement", request.LawsuitProofCreateInfo.GoodFaithStatement },
                { "CourtName", request.LawsuitProofCreateInfo.CourtName },
                { "CaseNumber", request.LawsuitProofCreateInfo.CaseNumber },
                { "FilingDate", request.LawsuitProofCreateInfo.FilingDate },
                { "Signature", request.LawsuitProofCreateInfo.Signature },
                { "LawsuitProofAttachFileKeys", DMCAAccusationId }
            };
            var startSagaTriggerMessage = _kafkaProducerService.PrepareStartSagaTriggerMessage(
                topic: SAGA_TOPIC,
                requestData: requestData,
                sagaInstanceId: null,
                messageName: "content-dmca-lawsuit-creation-flow");
            var result = await _messagingService.SendSagaMessageAsync(startSagaTriggerMessage);
            if (!result)
            {
                return StatusCode(500, "Failed to initiate dmca lawsuit proof submission.");
            }
            return Ok(new
            {
                SagaInstanceId = startSagaTriggerMessage.SagaInstanceId
            });
        }
        [HttpPut("{DMCAAccusationId}/assign/staffs/{AccountId}")]
        [Authorize(Policy = "Admin.BasicAccess")]
        public async Task<IActionResult> AssignDMCAAccusationToStaff(
            [FromRoute] int DMCAAccusationId,
            [FromRoute] int AccountId)
        {
            var requestData = new JObject
            {
                { "DMCAAccusationId", DMCAAccusationId },
                { "StaffAccountId", AccountId }
            };
            var startSagaTriggerMessage = _kafkaProducerService.PrepareStartSagaTriggerMessage(
                topic: "dmca-management-domain",
                requestData: requestData,
                sagaInstanceId: null,
                messageName: "content-dmca-staff-assignment-flow");
            var result = await _messagingService.SendSagaMessageAsync(startSagaTriggerMessage);
            if (!result)
            {
                return StatusCode(500, "Failed to initiate dmca staff assignment");
            }
            return Ok(new
            {
                SagaInstanceId = startSagaTriggerMessage.SagaInstanceId
            });
        }
        [HttpPut("staff/{DMCAAccusationId}")]
        [Authorize(Policy = "Staff.BasicAccess")]
        public async Task<IActionResult> UpdateDMCAAccusationById(
            [FromRoute] int DMCAAccusationId,
            [FromQuery] DMCAAccusationQueryEnum request)
        {
            var account = HttpContext.Items["LoggedInAccount"] as AccountStatusCache;
            var loginAccountId = account.Id;
            var requestData = new JObject
            {
                { "AccountId", loginAccountId },
                { "DMCAAccusationId", DMCAAccusationId },
                { "DMCAAccusationAction", (int)request },
            };
            var startSagaTriggerMessage = _kafkaProducerService.PrepareStartSagaTriggerMessage(
                topic: SAGA_TOPIC,
                requestData: requestData,
                sagaInstanceId: null,
                messageName: "update-dmca-accusation-status-flow");
            var result = await _messagingService.SendSagaMessageAsync(startSagaTriggerMessage);
            if (!result)
            {
                return StatusCode(500, "Failed to initiate dmca accusation update.");
            }
            return Ok(new
            {
                SagaInstanceId = startSagaTriggerMessage.SagaInstanceId
            });
        }
        [HttpPost("{DMCAAccusationId}/create-report")]
        [Authorize(Policy = "Staff.BasicAccess")]
        public async Task<IActionResult> CreateReportForDMCAAccusation(
            [FromRoute] int DMCAAccusationId,
            [FromBody] DMCAAccusationConclusionReportCreateRequestDTO request)
        {
            var account = HttpContext.Items["LoggedInAccount"] as AccountStatusCache;
            var loginAccountId = account.Id;
            var requestData = new JObject
            {
                { "AccountId", loginAccountId },
                { "DMCAAccusationId", DMCAAccusationId },
                { "DmcaAccusationConclusionReportTypeId", request.DmcaAccusationConclusionReportTypeId },
                { "Description", request.Description },
                { "InvalidReason", request.InvalidReason }
            };
            var startSagaTriggerMessage = _kafkaProducerService.PrepareStartSagaTriggerMessage(
                topic: SAGA_TOPIC,
                requestData: requestData,
                sagaInstanceId: null,
                messageName: "dmca-accusation-report-creation-flow");
            var result = await _messagingService.SendSagaMessageAsync(startSagaTriggerMessage);
            if (!result)
            {
                return StatusCode(500, "Failed to initiate dmca accusation report creation.");
            }
            return Ok(new
            {
                SagaInstanceId = startSagaTriggerMessage.SagaInstanceId
            });
        }
        [HttpPut("{DMCAAccusationConclusionReportId}/{IsValid}")]
        [Authorize(Policy = "Admin.BasicAccess")]
        public async Task<IActionResult> ValidateDMCAAccusationConclusionReport(
            [FromRoute] Guid DMCAAccusationConclusionReportId,
            [FromRoute] bool IsValid)
        {
            var requestData = new JObject
            {
                { "DMCAAccusationConclusionReportId", DMCAAccusationConclusionReportId },
                { "IsValid", IsValid }
            };
            var startSagaTriggerMessage = _kafkaProducerService.PrepareStartSagaTriggerMessage(
                topic: SAGA_TOPIC,
                requestData: requestData,
                sagaInstanceId: null,
                messageName: "dmca-accusation-report-validation-flow");
            var result = await _messagingService.SendSagaMessageAsync(startSagaTriggerMessage);
            if (!result)
            {
                return StatusCode(500, "Failed to initiate dmca accusation report validation.");
            }
            return Ok(new
            {
                SagaInstanceId = startSagaTriggerMessage.SagaInstanceId
            });
        }
        [HttpPut("{DMCAAccusationConclusionReportId}/cancel")]
        [Authorize(Policy = "Staff.BasicAccess")]
        public async Task<IActionResult> CancelDMCAAccusationConclusionReport(
            [FromRoute] Guid DMCAAccusationConclusionReportId)
        {
            var account = HttpContext.Items["LoggedInAccount"] as AccountStatusCache;
            var loginAccountId = account.Id;
            var requestData = new JObject
            {
                { "AccountId", loginAccountId },
                { "DMCAAccusationConclusionReportId", DMCAAccusationConclusionReportId }
            };
            var startSagaTriggerMessage = _kafkaProducerService.PrepareStartSagaTriggerMessage(
                topic: SAGA_TOPIC,
                requestData: requestData,
                sagaInstanceId: null,
                messageName: "dmca-accusation-report-cancellation-flow");
            var result = await _messagingService.SendSagaMessageAsync(startSagaTriggerMessage);
            if (!result)
            {
                return StatusCode(500, "Failed to initiate dmca accusation report cancellation.");
            }
            return Ok(new
            {
                SagaInstanceId = startSagaTriggerMessage.SagaInstanceId
            });
        }
    }
}
