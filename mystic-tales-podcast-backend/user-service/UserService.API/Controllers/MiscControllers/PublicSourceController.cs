using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UserService.API.Enums.Api;
using UserService.API.Filters.ExceptionFilters;
using UserService.BusinessLogic.DTOs.Cache;
using UserService.BusinessLogic.DTOs.MessageQueue.UserManagementDomain.SendUserServiceEmail;
using UserService.BusinessLogic.Enums.App;
using UserService.BusinessLogic.Helpers.FileHelpers;
using UserService.BusinessLogic.Models.CrossService;
using UserService.BusinessLogic.Models.Mail;
using UserService.BusinessLogic.Services.CrossServiceServices.QueryServices;
using UserService.BusinessLogic.Services.DbServices.MiscServices;
using UserService.Common.AppConfigurations.BusinessSetting.interfaces;
using UserService.Common.AppConfigurations.FilePath.interfaces;
using UserService.DataAccess.Data;

namespace UserService.API.Controllers.MiscControllers
{
    [Route("api/misc/public-source")]
    [ApiController]
    [TypeFilter(typeof(HttpExceptionFilter))]
    [Authorize(Policy = "OptionalAccess")]
    public class PublicSourceController : ControllerBase
    {
        private readonly ILogger<PublicSourceController> _logger;
        private readonly FileIOHelper _fileIOHelper;
        private readonly PdfFormFillingHelper _pdfFormFillingHelper;
        private readonly IFilePathConfig _filePathConfig;
        private readonly IMailPropertiesConfig _mailPropertiesConfig;
        private readonly MailOperationService _mailOperationService;
        public PublicSourceController(ILogger<PublicSourceController> logger, FileIOHelper fileIOHelper, PdfFormFillingHelper pdfFormFillingHelper, IFilePathConfig filePathConfig, IMailPropertiesConfig mailPropertiesConfig, MailOperationService mailOperationService)
        {
            _logger = logger;
            _fileIOHelper = fileIOHelper;
            _pdfFormFillingHelper = pdfFormFillingHelper;
            _filePathConfig = filePathConfig;
            _mailPropertiesConfig = mailPropertiesConfig;
            _mailOperationService = mailOperationService;
        }


        // /api/user-service/get-file-url/{**FileKey}
        [HttpGet("get-file-url/{**FileKey}")]
        public async Task<IActionResult> GetFileUrl(string FileKey)
        {
            // kiểm tra filkey có phải có pattern là "main_files/Bookings/<BookingId>/<BookingPodcastTrackId>_track_audio.<audio extension>" hoặc "main_files/PodcastEpisodes/<PodcastEpisodeId>/audio.<audio extension>" không, nếu có thì trả về exception 400
            // tức là sẽ có 2 loại fileKey không thể lấy url được từ url , các file còn lại thì được lấy binh thường
            // Console.WriteLine($"[DEBUG] Requested fileKey: {FileKey.StartsWith("main_files/Bookings/")}");
            // Console.WriteLine($"[DEBUG] Requested fileKey: {FileKey.Contains("_track_audio.")}");
            // Console.WriteLine($"[DEBUG] Requested fileKey: {FileKey.StartsWith("main_files/PodcastEpisodes/")}");
            // Console.WriteLine($"[DEBUG] Requested fileKey: {FileKey.Contains("/audio.")}");


            // Determine access level dựa trên user auth status
            var isAuthenticated = User.Identity?.IsAuthenticated ?? false;
            var requiredLevel = isAuthenticated ? FileAccessLevelEnum.RequiresAuth : FileAccessLevelEnum.Public;

            var validation = FileAccessValidator.ValidateFileAccess(FileKey, requiredLevel);

            if (!validation.IsValid)
            {
                return StatusCode(403, new { error = validation.ErrorMessage });
            }

            // Additional ownership checks...

            var url = await _fileIOHelper.GeneratePresignedUrlAsync(FileKey);
            return Ok(new { FileUrl = url });
        }

        // /api/user-service/api/misc/public-source/podcaster-documents/{PodcasterDocumentFileType}/get-file-url
        [HttpGet("podcaster-documents/{PodcasterDocumentFileType}/get-file-url")]
        [Authorize(Policy = "AdminOrStaffOrCustomer.BasicAccess")]
        public async Task<IActionResult> GetPodcasterDocumentTemplateFileUrl(PodcasterDocumentFileTypeEnum PodcasterDocumentFileType)
        {
            var account = HttpContext.Items["LoggedInAccount"] as AccountStatusCache;

            if (PodcasterDocumentFileType == PodcasterDocumentFileTypeEnum.MainBuddyCommitmentDocumentTemplate)
            {
                var fileKey = $"{_filePathConfig.SYSTEM_PODCASTER_DOCUMENTS_FILE_PATH}/main_buddy_commitment_document_template_0.pdf";
                var url = await _fileIOHelper.GeneratePresignedUrlAsync(fileKey, 120);
                return Ok(new { FileUrl = url });
            }
            return BadRequest("Invalid PodcasterDocumentFileType.");
        }

        // /api/user-service/api/misc/public-source/podcaster-documents/buddy-commitment-document-template/fill-podcaster-apply-info/get-file-content
        [HttpGet("podcaster-documents/buddy-commitment-document-template/fill-podcaster-apply-info/get-file-content")]
        [Authorize(Policy = "Customer.NonPodcasterAccess")]
        public async Task<IActionResult> GetFilledInfoBuddyCommitmentDocumentTemplateFileContent()
        {
            try
            {
                var account = HttpContext.Items["LoggedInAccount"] as AccountStatusCache;

                // 1. Get template PDF from S3
                string templatePath = $"{_filePathConfig.SYSTEM_PODCASTER_DOCUMENTS_FILE_PATH}/main_buddy_commitment_document_template_2.pdf";

                byte[]? templateBytes = await _fileIOHelper.GetFileBytesAsync(templatePath);

                if (templateBytes == null || templateBytes.Length == 0)
                {
                    _logger.LogError($"Template PDF not found at: {templatePath}");
                    return NotFound(new { message = "PDF template not found" });
                }

                _logger.LogInformation($"Template PDF loaded: {templateBytes.Length} bytes");

                // 2. Prepare field values to fill
                var fieldValues = new Dictionary<string, string>
            {
                { "mail_day_ne", account.Email }, // Fill email into 'mail_day_ne' field
                // Add more fields as needed:
                // { "date_field", DateTime.Now.ToString("dd/MM/yyyy") },
                // { "name_field", account.FullName },
            };

                _logger.LogInformation($"Filling PDF with email: {account.Email}");

                // 3. Fill PDF form fields
                byte[] filledPdfBytes = _pdfFormFillingHelper.FillPdfTextFormFields(
                    templateBytes,
                    fieldValues,
                    flattenForm: false // Lock the form after filling
                );

                _logger.LogInformation($"PDF filled successfully: {filledPdfBytes.Length} bytes");

                // 4. Return filled PDF
                string fileName = $"commitment_document_{account.Id}_{DateTime.Now:yyyyMMddHHmmss}.pdf";

                return File(filledPdfBytes, "application/pdf", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error generating filled PDF: {ex.Message}");
                _logger.LogError($"Stack trace: {ex.StackTrace}");
                return StatusCode(500, new
                {
                    message = "Failed to generate PDF",
                    error = ex.Message
                });
            }
        }


        [HttpGet("podcaster-documents/debug/list-form-fields")]
        [AllowAnonymous] // For testing, remove in production
        public async Task<IActionResult> DebugListFormFields()
        {
            try
            {
                string templatePath = $"{_filePathConfig.SYSTEM_PODCASTER_DOCUMENTS_FILE_PATH}/main_buddy_commitment_document_template_2.pdf";

                byte[]? templateBytes = await _fileIOHelper.GetFileBytesAsync(templatePath);

                if (templateBytes == null)
                {
                    return NotFound(new { message = "Template not found" });
                }

                var fieldNames = _pdfFormFillingHelper.GetFormFieldNames(templateBytes);

                return Ok(new
                {
                    message = "Form fields found",
                    templatePath = templatePath,
                    totalFields = fieldNames.Count,
                    fields = fieldNames
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }


        // /api/user-service/api/misc/public-source/test-email?templateName=YourTemplateName
        [HttpPost("test-email")]
        public async Task<IActionResult> TestEmail([FromBody] SendUserServiceEmailMailInfoDTO mailInfo)
        {

            object mailModel = mailInfo.MailTypeName switch
            {
                "CustomerRegistrationVerification" => mailInfo.MailObject.ToObject<CustomerRegistrationVerificationMailViewModel>(),
                "CustomerPasswordReset" => mailInfo.MailObject.ToObject<CustomerPasswordResetMailViewModel>(),
                "PodcasterRequestConfirmation" => mailInfo.MailObject.ToObject<PodcasterRequestConfirmationMailViewModel>(),
                "PodcasterRequestResult" => mailInfo.MailObject.ToObject<PodcasterRequestResultMailViewModel>(),
                "CustomerGoogleRegistrationNewAccountPassword" => mailInfo.MailObject.ToObject<CustomerGoogleRegistrationNewAccountPasswordMailViewModel>(),
                _ => mailInfo.MailObject.ToObject<object>()
            };

            Console.WriteLine("Sending email to: " + mailInfo.MailObject["VerifyCode"]);
            var mailProperty = _mailPropertiesConfig.GetMailPropertyByTypeName(mailInfo.MailTypeName);
            await _mailOperationService.SendUserServiceEmail(mailProperty, mailInfo.ToEmail, mailModel);
            return Ok(new { message = "Test email sent." });
        }
    }
}
