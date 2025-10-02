using Microsoft.AspNetCore.Mvc;
using SurveyTalkService.Common.AppConfigurations.BusinessSetting.interfaces;
using SurveyTalkService.Common.Helpers;

namespace SurveyTalkService.API.Controllers.MiscControllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FileValidationController : ControllerBase
    {
        private readonly IFileValidationConfig _fileValidationConfig;

        public FileValidationController(IFileValidationConfig fileValidationConfig)
        {
            _fileValidationConfig = fileValidationConfig;
        }

        /// <summary>
        /// Get all available validation rules
        /// </summary>
        [HttpGet("rules")]
        public ActionResult<object> GetValidationRules()
        {
            var rules = new[]
            {
                "Account.mainImageFileUrl",
                "Account.profileImageFileUrl", 
                "Account.CVFileUrl",
                "Account.PassportFileUrl",
                "Account.IdentityCardFileUrl",
                "Account.DrivingLicenseFileUrl",
                "Account.BirthCertificateFileUrl",
                "Account.HealthInsuranceFileUrl",
                "Account.ContractFileUrl",
                "Account.AudioRecordingFileUrl",
                "Account.PresentationFileUrl",
                "Account.ReportFileUrl",
                "Account.SpreadsheetFileUrl",
                "Account.BackupFileUrl",
                "Account.LogFileUrl",
                "Account.CertificateFileUrl",
                "Account.LicenseFileUrl",
                "Account.LegalDocumentFileUrl",
                "Account.OtherFileUrl"
            }.Select(fieldName => new
            {
                FieldName = fieldName,
                Rule = _fileValidationConfig.GetValidationRule(fieldName)
            }).Where(x => x.Rule != null);

            return Ok(rules);
        }

        /// <summary>
        /// Get validation rule for specific field
        /// </summary>
        [HttpGet("rules/{fieldName}")]
        public ActionResult<object> GetValidationRule(string fieldName)
        {
            var rule = _fileValidationConfig.GetValidationRule(fieldName);
            if (rule == null)
            {
                return NotFound($"No validation rule found for field: {fieldName}");
            }

            return Ok(new
            {
                FieldName = fieldName,
                Rule = rule
            });
        }

        /// <summary>
        /// Validate a file upload
        /// </summary>
        [HttpPost("validate")]
        public ActionResult<object> ValidateFile([FromForm] FileValidationRequest request)
        {
            if (request.File == null || request.File.Length == 0)
            {
                return BadRequest("No file provided");
            }

            var mimeType = request.File.ContentType ?? FileValidationHelper.GetMimeType(request.File.FileName);
            
            var result = FileValidationHelper.ValidateFile(
                _fileValidationConfig,
                request.FieldName,
                request.File.FileName,
                request.File.Length,
                mimeType
            );

            return Ok(new
            {
                IsValid = result.IsValid,
                ErrorMessage = result.ErrorMessage,
                FileInfo = new
                {
                    FileName = request.File.FileName,
                    Size = FileValidationHelper.FormatFileSize(request.File.Length),
                    SizeBytes = request.File.Length,
                    MimeType = mimeType,
                    Extension = System.IO.Path.GetExtension(request.File.FileName)
                },
                Rule = result.Rule
            });
        }

        /// <summary>
        /// Test validation without file upload
        /// </summary>
        [HttpPost("test")]
        public ActionResult<object> TestValidation([FromBody] TestValidationRequest request)
        {
            var result = FileValidationHelper.ValidateFile(
                _fileValidationConfig,
                request.FieldName,
                request.FileName,
                request.FileSizeBytes,
                request.MimeType
            );

            return Ok(new
            {
                IsValid = result.IsValid,
                ErrorMessage = result.ErrorMessage,
                FileInfo = new
                {
                    FileName = request.FileName,
                    Size = FileValidationHelper.FormatFileSize(request.FileSizeBytes),
                    SizeBytes = request.FileSizeBytes,
                    MimeType = request.MimeType,
                    Extension = System.IO.Path.GetExtension(request.FileName)
                },
                Rule = result.Rule
            });
        }
    }

    public class FileValidationRequest
    {
        public required string FieldName { get; set; }
        public required IFormFile File { get; set; }
    }

    public class TestValidationRequest
    {
        public required string FieldName { get; set; }
        public required string FileName { get; set; }
        public required long FileSizeBytes { get; set; }
        public required string MimeType { get; set; }
    }
}