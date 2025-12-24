using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SubscriptionService.API.Filters.ExceptionFilters;
using SubscriptionService.BusinessLogic.DTOs.MessageQueue.SubscriptionManagementDomain.SendSubscriptionServiceEmail;
using SubscriptionService.BusinessLogic.Enums.App;
using SubscriptionService.BusinessLogic.Helpers.FileHelpers;
using SubscriptionService.BusinessLogic.Models.CrossService;
using SubscriptionService.BusinessLogic.Models.Mail;
using SubscriptionService.BusinessLogic.Services.CrossServiceServices.QueryServices;
using SubscriptionService.BusinessLogic.Services.DbServices.MiscServices;
using SubscriptionService.Common.AppConfigurations.BusinessSetting.interfaces;
using SubscriptionService.DataAccess.Data;

namespace SubscriptionService.API.Controllers.MiscControllers
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
            // kiểm tra FileKey có phải có pattern là "main_files/Bookings/<BookingId>/<BookingPodcastTrackId>_track_audio.<audio extension>" hoặc "main_files/PodcastEpisodes/<PodcastEpisodeId>/audio.<audio extension>" không, nếu có thì trả về exception 400
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


        // /api/subscription-service/api/misc/public-source/test-email
        [HttpPost("test-email")]
        public async Task<IActionResult> TestEmail([FromBody] SendSubscriptionServiceEmailInfoDTO mailInfo)
        {

            object mailModel = mailInfo.MailTypeName switch
            {
                "PodcastSubscriptionRegistration" => mailInfo.MailObject.ToObject<PodcastSubscriptionRegistrationMailViewModel>(),
                "PodcastSubscriptionNewVersion" => mailInfo.MailObject.ToObject<PodcastSubscriptionNewVersionMailViewModel>(),
                "PodcastSubscriptionRegistrationRenewalSuccess" => mailInfo.MailObject.ToObject<PodcastSubscriptionRegistrationRenewalSuccessMailViewModel>(),
                "PodcastSubscriptionRegistrationRenewalFailure" => mailInfo.MailObject.ToObject<PodcastSubscriptionRegistrationRenewalFailureMailViewModel>(),
                "PodcastSubscriptionRegistrationCancel" => mailInfo.MailObject.ToObject<PodcastSubscriptionRegistrationCancelMailViewModel>(),
                "PodcastSubscriptionCancel" => mailInfo.MailObject.ToObject<PodcastSubscriptionCancelMailViewModel>(),
                "PodcastSubscriptionInactive" => mailInfo.MailObject.ToObject<PodcastSubscriptionInactiveMailViewModel>(),
                "PodcastSubscriptionDuplicate" => mailInfo.MailObject.ToObject<PodcastSubscriptionDuplicateMailViewModel>(),
                _ => mailInfo.MailObject.ToObject<object>()
            };

            Console.WriteLine("Sending email to: " + mailInfo.MailObject["VerifyCode"]);
            var mailProperty = _mailPropertiesConfig.GetMailPropertyByTypeName(mailInfo.MailTypeName);
            await _mailOperationService.SendSubscriptionServiceEmail(mailProperty, mailInfo.ToEmail, mailModel);
            return Ok(new { message = "Test email sent." });
        }
    }
}
