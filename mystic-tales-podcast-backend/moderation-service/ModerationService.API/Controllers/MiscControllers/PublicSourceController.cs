using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ModerationService.API.Filters.ExceptionFilters;
using ModerationService.BusinessLogic.DTOs.MessageQueue.ReportManagementDomain.SendModerationServiceEmail;
using ModerationService.BusinessLogic.Enums.App;
using ModerationService.BusinessLogic.Helpers.FileHelpers;
using ModerationService.BusinessLogic.Models.CrossService;
using ModerationService.BusinessLogic.Models.Mail;
using ModerationService.BusinessLogic.Services.CrossServiceServices.QueryServices;
using ModerationService.BusinessLogic.Services.DbServices.MiscServices;
using ModerationService.Common.AppConfigurations.BusinessSetting.interfaces;
using ModerationService.DataAccess.Data;

namespace ModerationService.API.Controllers.MiscControllers
{
    [Route("api/misc/public-source")]
    [ApiController]
    [TypeFilter(typeof(HttpExceptionFilter))]
    [Authorize(Policy = "OptionalAccess")]
    public class PublicSourceController : ControllerBase
    {
        private readonly ILogger<PublicSourceController> _logger;
        private readonly FileIOHelper _fileIOHelper;
        private readonly IMailPropertiesConfig _mailPropertiesConfig;
        private readonly MailOperationService _mailOperationService;
        public PublicSourceController(ILogger<PublicSourceController> logger, FileIOHelper fileIOHelper, IMailPropertiesConfig mailPropertiesConfig, MailOperationService mailOperationService)
        {
            _logger = logger;
            _fileIOHelper = fileIOHelper;
            _mailPropertiesConfig = mailPropertiesConfig;
            _mailOperationService = mailOperationService;
        }


        // /api/user-service/get-file-url/{**FileKey}
        [HttpGet("get-file-url/{**FileKey}")]
        public async Task<IActionResult> GetFileUrl(string FileKey)
        {
            // kiểm tra filkey có phải có pattern là "main_files/Bookings/<BookingId>/<BookingPodcastTrackId>_track_audio.<audio extension>" hoặc "main_files/PodcastEpisodes/<PodcastEpisodeId>/audio.<audio extension>" không, nếu có thì trả về exception 400
            // tức là sẽ có 2 loại FileKey không thể lấy url được từ url , các file còn lại thì được lấy binh thường
            // Console.WriteLine($"[DEBUG] Requested FileKey: {FileKey.StartsWith("main_files/Bookings/")}");
            // Console.WriteLine($"[DEBUG] Requested FileKey: {FileKey.Contains("_track_audio.")}");
            // Console.WriteLine($"[DEBUG] Requested FileKey: {FileKey.StartsWith("main_files/PodcastEpisodes/")}");
            // Console.WriteLine($"[DEBUG] Requested FileKey: {FileKey.Contains("/audio.")}");


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


        // /api/moderation-service/api/misc/public-source/test-email
        [HttpPost("test-email")]
        public async Task<IActionResult> TestEmail([FromBody] SendModerationServiceEmailInfoDTO mailInfo)
        {

            object mailModel = mailInfo.MailTypeName switch
            {
                "DMCANoticePending" => mailInfo.MailObject.ToObject<DMCANoticePendingMailViewModel>(),
                "DMCACounterNoticePending" => mailInfo.MailObject.ToObject<DMCACounterNoticePendingMailViewModel>(),
                "DMCALawsuitProofPending" => mailInfo.MailObject.ToObject<DMCALawsuitProofPendingMailViewModel>(),
                "DMCANoticeInvalid" => mailInfo.MailObject.ToObject<DMCANoticeInvalidMailViewModel>(),
                "DMCACounterNoticeInvalidToAccused" => mailInfo.MailObject.ToObject<DMCACounterNoticeInvalidToAccusedMailViewModel>(),
                "DMCACounterNoticeInvalidToAccuser" => mailInfo.MailObject.ToObject<DMCACounterNoticeInvalidToAccuserMailViewModel>(),
                "DMCALawsuitProofInvalidToAccused" => mailInfo.MailObject.ToObject<DMCALawsuitProofInvalidToAccusedMailViewModel>(),
                "DMCALawsuitProofInvalidToAccuser" => mailInfo.MailObject.ToObject<DMCALawsuitProofInvalidToAccuserMailViewModel>(),
                "DMCALawsuitProofPodcasterWinToAccused" => mailInfo.MailObject.ToObject<DMCALawsuitProofPodcasterWinToAccusedMailViewModel>(),
                "DMCALawsuitProofPodcasterWinToAccuser" => mailInfo.MailObject.ToObject<DMCALawsuitProofPodcasterWinToAccuserMailViewModel>(),
                "DMCALawsuitProofAccuserWinToAccused" => mailInfo.MailObject.ToObject<DMCALawsuitProofAccuserWinToAccusedMailViewModel>(),
                "DMCALawsuitProofAccuserWinToAccuser" => mailInfo.MailObject.ToObject<DMCALawsuitProofAccuserWinToAccuserMailViewModel>(),
                "DMCANoticeValidToAccuser" => mailInfo.MailObject.ToObject<DMCANoticeValidToAccuserMailViewModel>(),
                "DMCANoticeValidToAccused" => mailInfo.MailObject.ToObject<DMCANoticeValidToAccusedMailViewModel>(),
                "DMCANoticeValidNotResponseInTimeToAccused" => mailInfo.MailObject.ToObject<DMCANoticeValidNotResponseInTimeToAccusedMailViewModel>(),
                "DMCANoticeValidNotResponseInTimeToAccuser" => mailInfo.MailObject.ToObject<DMCANoticeValidNotResponseInTimeToAccuserMailViewModel>(),
                "DMCANoticeValidAgreeTakenDownToAccused" => mailInfo.MailObject.ToObject<DMCANoticeValidAgreeTakenDownToAccusedMailViewModel>(),
                "DMCANoticeValidAgreeTakenDownToAccuser" => mailInfo.MailObject.ToObject<DMCANoticeValidAgreeTakenDownToAccuserMailViewModel>(),
                "DMCACounterNoticeConfirmation" => mailInfo.MailObject.ToObject<DMCACounterNoticeConfirmationMailViewModel>(),
                "DMCACounterNoticeValidToAccused" => mailInfo.MailObject.ToObject<DMCACounterNoticeValidToAccusedMailViewModel>(),
                "DMCACounterNoticeValidToAccuser" => mailInfo.MailObject.ToObject<DMCACounterNoticeValidToAccuserMailViewModel>(),
                "DMCACounterNoticeValidNotResponseInTimeToAccused" => mailInfo.MailObject.ToObject<DMCACounterNoticeValidNotResponseInTimeToAccusedMailViewModel>(),
                "DMCACounterNoticeValidNotResponseInTimeToAccuser" => mailInfo.MailObject.ToObject<DMCACounterNoticeValidNotResponseInTimeToAccuserMailViewModel>(),
                "DMCALawsuitProofValidToAccused" => mailInfo.MailObject.ToObject<DMCALawsuitProofValidToAccusedMailViewModel>(),
                "DMCALawsuitProofValidToAccuser" => mailInfo.MailObject.ToObject<DMCALawsuitProofValidToAccuserMailViewModel>(),
                _ => mailInfo.MailObject.ToObject<object>()
            };

            Console.WriteLine("Sending email to: " + mailInfo.MailObject["VerifyCode"]);
            var mailProperty = _mailPropertiesConfig.GetMailPropertyByTypeName(mailInfo.MailTypeName);
            await _mailOperationService.SendModerationServiceEmail(mailProperty, mailInfo.ToEmail, mailModel);
            return Ok(new { message = "Test email sent." });
        }
    }
}
