using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UserService.API.Filters.ExceptionFilters;
using UserService.BusinessLogic.DTOs.Account;
using UserService.BusinessLogic.DTOs.Cache;
using UserService.BusinessLogic.Helpers.FileHelpers;
using UserService.BusinessLogic.Models.CrossService;
using UserService.BusinessLogic.Services.CrossServiceServices.QueryServices;
using UserService.BusinessLogic.Services.DbServices.UserServices;
using UserService.BusinessLogic.Services.MessagingServices.interfaces;
using UserService.Common.AppConfigurations.BusinessSetting.interfaces;
using UserService.Common.AppConfigurations.FilePath.interfaces;
using UserService.DataAccess.Data;
using UserService.Infrastructure.Services.Kafka;
using UserService.Infrastructure.Services.Redis;

namespace UserService.API.Controllers.BaseControllers
{
    [Route("api/accounts")]
    [ApiController]
    [TypeFilter(typeof(HttpExceptionFilter))]
    public class AccountController : ControllerBase
    {
        private readonly KafkaProducerService _kafkaProducerService;
        private readonly IMessagingService _messagingService;
        private readonly IFileValidationConfig _fileValidationConfig;
        private readonly IFilePathConfig _filePathConfig;
        private readonly FileIOHelper _fileIOHelper;
        private readonly AccountService _accountService;
        private readonly RedisInstanceCacheService _redisInstanceCacheService;
        private readonly RedisSharedCacheService _redisSharedCacheService;

        public AccountController(FileIOHelper fileIOHelper, KafkaProducerService kafkaProducerService, IMessagingService messagingService, IFileValidationConfig fileValidationConfig, IFilePathConfig filePathConfig, AccountService accountService, RedisInstanceCacheService redisInstanceCacheService, RedisSharedCacheService redisSharedCacheService)
        {
            _fileIOHelper = fileIOHelper;
            _kafkaProducerService = kafkaProducerService;
            _messagingService = messagingService;
            _fileValidationConfig = fileValidationConfig;
            _accountService = accountService;
            _filePathConfig = filePathConfig;
            _redisInstanceCacheService = redisInstanceCacheService;
            _redisSharedCacheService = redisSharedCacheService;
        }

        // [HttpGet("roles")]
        // public async Task<IActionResult> GetRoles()
        // {
        //     var roles = await _appDbContext.Roles.ToListAsync();
        //     return Ok(roles);
        // }

        [HttpGet("get-all-redis-instance-key-values")]
        public async Task<IActionResult> GetAllRedisInstanceKeyValues()
        {
            var keys = await _redisInstanceCacheService.GetAllKeyValuesAsync();
            return Ok(keys);
        }

        [HttpGet("get-all-redis-shared-key-values")]
        public async Task<IActionResult> GetAllRedisSharedKeyValues()
        {
            var keys = await _redisSharedCacheService.GetAllKeyValuesAsync();
            return Ok(keys);
        }

        [HttpPost("test-account-status")]
        public async Task<IActionResult> TestAccountStatus([FromBody] JToken changeAccountStatusParameter)
        {
            var requestData = JObject.FromObject(new
            {
                Id = changeAccountStatusParameter["Id"]
            });
            var startSagaTriggerMessage = _kafkaProducerService.PrepareStartSagaTriggerMessage("user-management-domain", requestData, null, "account-status-change-flow");
            await _messagingService.SendSagaMessageAsync(startSagaTriggerMessage);
            return Ok(new
            {
                SagaInstanceId = startSagaTriggerMessage.SagaInstanceId
            }
            );
        }

        [HttpGet("test-get-account-status")]
        [Authorize(Policy = "Customer.NoViolationAccess")]
        public async Task<IActionResult> TestGetAccountStatus()
        {
            var account = HttpContext.Items["LoggedInAccount"] as AccountStatusCache;
            return Ok(new
            {
                Account = account,
                Message = $"Hello, your account ID is {account.Id}, RoleId is {account.RoleId}, ViolationLevel is {account.ViolationLevel}, ViolationPoint is {account.ViolationPoint}, IsVerified is {account.IsVerified}, DeactivatedAt is {account.DeactivatedAt}, LastViolationLevelChanged is {account.LastViolationLevelChanged}, LastViolationPointChanged is {account.LastViolationPointChanged}"
            });
        }

        // /api/user-service/get-file-url/{fileKey}
        [HttpGet("get-file-url/{**fileKey}")]
        [Authorize]
        public async Task<IActionResult> GetFileUrl(string fileKey)
        {
            // kiểm tra filkey có phải có pattern là "main_files/Bookings/<BookingId>/<BookingPodcastTrackId>_track_audio.<audio extension>" hoặc "main_files/PodcastEpisodes/<PodcastEpisodeId>/audio.<audio extension>" không, nếu có thì trả về exception 400
            // tức là sẽ có 2 loại fileKey không thể lấy url được từ url , các file còn lại thì được lấy binh thường
            if (fileKey.StartsWith("main_files/Bookings/") && fileKey.Contains("_track_audio."))
            {
                return Forbid("Cannot get URL for booking track audio files.");
            }
            if (fileKey.StartsWith("main_files/PodcastEpisodes/") && fileKey.Contains("/audio."))
            {
                return Forbid("Cannot get URL for podcast episode audio files.");
            }
            var fileUrl = await _fileIOHelper.GeneratePresignedUrlAsync(fileKey);

            return Ok(new { FileUrl = fileUrl });
        }

        // /api/user-service/api/accounts/customers
        [HttpGet("customers")]
        public async Task<IActionResult> GetCustomers()
        {
            var customers = await _accountService.GetCustomerAccounts();

            return Ok(new { CustomerList = customers });
        }

        // /api/user-service/api/accounts/staffs?IsDeactivated
        [HttpGet("staffs")]
        public async Task<IActionResult> GetStaffs([FromQuery] bool? IsDeactivated = null)
        {
            var staffs = await _accountService.GetStaffAccounts(IsDeactivated);

            return Ok(new { StaffList = staffs });
        }

        // /api/user-service/api/accounts/podcasters
        [HttpGet("podcasters")]
        [Authorize(Policy = "Admin.BasicAccess")]
        public async Task<IActionResult> GetPodcasters()
        {
            var podcasters = await _accountService.GetPodcasterAccounts();

            return Ok(new { PodcasterList = podcasters });
        }

        // /api/user-service/api/accounts/podcast-buddies
        [HttpGet("podcast-buddies")]
        [Authorize(Policy = "AdminOrStaffOrCustomer.BasicAccess")]
        public async Task<IActionResult> GetPodcastBuddies()
        {
            var account = HttpContext.Items["LoggedInAccount"] as AccountStatusCache;
            var podcastBuddies = await _accountService.GetPodcastBuddyAccounts(account.RoleId);

            return Ok(new { PodcastBuddyList = podcastBuddies });
        }

        // /api/user-service/api/accounts/podcaster/apply
        [HttpPost("podcaster/apply")]
        [Authorize(Policy = "Customer.NoViolationAccess.NonPodcasterAccess")]

        public async Task<IActionResult> ApplyPodcaster([FromForm] PodcasterProfileRequestDTO podcasterProfileRequestDTO)
        {
            var account = HttpContext.Items["LoggedInAccount"] as AccountStatusCache;

            var podcasterProfileRequestInfo = JsonConvert.DeserializeObject<PodcasterProfileRequestInfoDTO>(podcasterProfileRequestDTO.PodcasterProfileRequestInfo);

            string commitmentDocumentFileKey = null;
            if (podcasterProfileRequestDTO.CommitmentDocumentFile != null)
            {
                // bool IsValidFile(string fieldName, string fileName, long fileSizeBytes, string mimeType);
                var isValidFile = _fileValidationConfig.IsValidFile("PodcasterProfile.commitmentDocumentFileKey", podcasterProfileRequestDTO.CommitmentDocumentFile.FileName, podcasterProfileRequestDTO.CommitmentDocumentFile.Length, podcasterProfileRequestDTO.CommitmentDocumentFile.ContentType);
                if (!isValidFile)
                {
                    return BadRequest("Invalid upload file.");
                }

                string newCommitmentDocumentFileName = $"{Guid.NewGuid()}_{podcasterProfileRequestDTO.CommitmentDocumentFile.FileName}";
                using (var stream = podcasterProfileRequestDTO.CommitmentDocumentFile.OpenReadStream())
                {
                    await _fileIOHelper.UploadBinaryFileWithStreamAsync(
                                        stream,
                                        _filePathConfig.ACCOUNT_TEMP_FILE_PATH,
                                        newCommitmentDocumentFileName
                                    );
                }
                commitmentDocumentFileKey = FilePathHelper.CombinePaths(_filePathConfig.ACCOUNT_TEMP_FILE_PATH, newCommitmentDocumentFileName);

            }
            JObject requestData = JObject.FromObject(podcasterProfileRequestInfo);
            requestData["AccountId"] = account.Id;
            requestData["CommitmentDocumentFileKey"] = commitmentDocumentFileKey;



            var startSagaTriggerMessage = _kafkaProducerService.PrepareStartSagaTriggerMessage("user-management-domain", requestData, null, "podcaster-profile-creation-flow");
            await _messagingService.SendSagaMessageAsync(startSagaTriggerMessage);
            return Ok(new
            {
                SagaInstanceId = startSagaTriggerMessage.SagaInstanceId
            }
            );
        }

        // /api/user-service/api/accounts/podcaster/{AccountId}
        [HttpGet("podcaster/{AccountId}")]
        [Authorize(Policy = "AdminOrStaffOrCustomer.BasicAccess")]
        public async Task<IActionResult> GetPodcasterProfileByAccountId(int AccountId)
        {
            var podcasterProfile = await _accountService.GetPodcasterProfileByAccountId(AccountId);
            var account = HttpContext.Items["LoggedInAccount"] as AccountStatusCache;
            if (account.RoleId == 1 && podcasterProfile.Id != account.Id)
            {
                return StatusCode(403, "Customer accounts can only view their own podcaster profiles.");
            }
            return Ok(podcasterProfile);
        }

        // /api/user-service/api/accounts/podcast-buddies/{AccountId}
        [HttpGet("podcast-buddies/{AccountId}")]
        [Authorize(Policy = "AdminOrStaffOrCustomer.BasicAccess")]
        public async Task<IActionResult> GetPodcastBuddyProfileByAccountId(int AccountId)
        {
            var account = HttpContext.Items["LoggedInAccount"] as AccountStatusCache;
            var podcastBuddyProfile = await _accountService.GetPodcastBuddyProfileByAccountId(AccountId, account.RoleId);

            return Ok(podcastBuddyProfile);
        }

        // /api/user-service/api/accounts/podcaster/{AccountId}
        [HttpPut("podcaster/{AccountId}")]
        [Authorize(Policy = "Customer.BasicAccess")]
        public async Task<IActionResult> UpdatePodcasterProfileByAccountId(PodcasterProfileUpdateRequestDTO podcasterProfileUpdateRequestDTO, int AccountId)
        {
            var account = HttpContext.Items["LoggedInAccount"] as AccountStatusCache;
            if (account.RoleId != 1)
            {
                return StatusCode(403, "Only customer accounts can update their podcaster profiles.");
            }
            else if (account.Id != AccountId)
            {
                return StatusCode(403, "You can only update your own podcaster profile.");
            }

            var podcasterProfileRequestInfo = JsonConvert.DeserializeObject<PodcasterProfileRequestInfoDTO>(podcasterProfileUpdateRequestDTO.PodcasterProfileRequestInfo);

            string buddyAudioFileKey = null;
            if (podcasterProfileUpdateRequestDTO.BuddyAudioFile != null)
            {
                // bool IsValidFile(string fieldName, string fileName, long fileSizeBytes, string mimeType);
                var isValidFile = _fileValidationConfig.IsValidFile("PodcasterProfile.buddyAudioFileKey", podcasterProfileUpdateRequestDTO.BuddyAudioFile.FileName, podcasterProfileUpdateRequestDTO.BuddyAudioFile.Length, podcasterProfileUpdateRequestDTO.BuddyAudioFile.ContentType);
                if (!isValidFile)
                {
                    return BadRequest("Invalid upload file.");
                }

                string newBuddyAudioFileName = $"{Guid.NewGuid()}_{podcasterProfileUpdateRequestDTO.BuddyAudioFile.FileName}";
                using (var stream = podcasterProfileUpdateRequestDTO.BuddyAudioFile.OpenReadStream())
                {
                    await _fileIOHelper.UploadBinaryFileWithStreamAsync(
                                        stream,
                                        _filePathConfig.ACCOUNT_TEMP_FILE_PATH,
                                        newBuddyAudioFileName
                                    );
                }
                buddyAudioFileKey = FilePathHelper.CombinePaths(_filePathConfig.ACCOUNT_TEMP_FILE_PATH, newBuddyAudioFileName);

            }
            JObject requestData = JObject.FromObject(podcasterProfileRequestInfo);
            requestData["BuddyAudioFileKey"] = buddyAudioFileKey;
            requestData["AccountId"] = AccountId;

            var startSagaTriggerMessage = _kafkaProducerService.PrepareStartSagaTriggerMessage("user-management-domain", requestData, null, "podcaster-profile-update-flow");
            await _messagingService.SendSagaMessageAsync(startSagaTriggerMessage);
            return Ok(new
            {
                SagaInstanceId = startSagaTriggerMessage.SagaInstanceId
            });
        }

        // /api/user-service/api/accounts/{AccountId}
        [HttpGet("{AccountId}")]
        [Authorize]
        public async Task<IActionResult> GetAccountById(int AccountId)
        {
            var account = await _accountService.GetAccountById(AccountId);
            var requestingAccount = HttpContext.Items["LoggedInAccount"] as AccountStatusCache;
            if (requestingAccount.RoleId == 1 && account.Id != requestingAccount.Id)
            {
                return StatusCode(403, "Customer accounts can only view their own account details.");
            }
            return Ok(account);
        }

        // /api/user-service/api/accounts/{AccountId}
        [HttpPut("{AccountId}")]
        [Authorize(Policy = "AdminOrCustomer.BasicAccess")]
        public async Task<IActionResult> UpdateAccountById(AccountUpdateRequestDTO accountUpdateRequestDTO, int AccountId)
        {
            var account = HttpContext.Items["LoggedInAccount"] as AccountStatusCache;
            if (account.RoleId == 1 && account.Id != AccountId)
            {
                return StatusCode(403, "Customer accounts can only update their own account details.");
            }
            var accountUpdateInfo = JsonConvert.DeserializeObject<AccountUpdateInfoDTO>(accountUpdateRequestDTO.AccountUpdateInfo);

            string mainImageFileKey = null;
            if (accountUpdateRequestDTO.MainImageFile != null)
            {
                // bool IsValidFile(string fieldName, string fileName, long fileSizeBytes, string mimeType);
                var isValidImage = _fileValidationConfig.IsValidFile("Account.mainImageFileKey", accountUpdateRequestDTO.MainImageFile.FileName, accountUpdateRequestDTO.MainImageFile.Length, accountUpdateRequestDTO.MainImageFile.ContentType);
                if (!isValidImage)
                {
                    return BadRequest("Invalid image file.");
                }

                string newMainImageFileName = $"{Guid.NewGuid()}_{accountUpdateRequestDTO.MainImageFile.FileName}";
                using (var stream = accountUpdateRequestDTO.MainImageFile.OpenReadStream())
                {
                    await _fileIOHelper.UploadBinaryFileWithStreamAsync(
                                        stream,
                                        _filePathConfig.ACCOUNT_TEMP_FILE_PATH,
                                        newMainImageFileName
                                    );
                }
                mainImageFileKey = FilePathHelper.CombinePaths(_filePathConfig.ACCOUNT_TEMP_FILE_PATH, newMainImageFileName);

            }
            JObject requestData = JObject.FromObject(accountUpdateInfo);
            requestData["MainImageFileKey"] = mainImageFileKey;
            requestData["AccountId"] = AccountId;

            var startSagaTriggerMessage = _kafkaProducerService.PrepareStartSagaTriggerMessage("user-management-domain", requestData, null, "user-update-flow");
            await _messagingService.SendSagaMessageAsync(startSagaTriggerMessage);
            return Ok(new
            {
                SagaInstanceId = startSagaTriggerMessage.SagaInstanceId
            }
            );
        }

        // /api/user-service/api/accounts/{AccountId}/deactivate/{IsDeactivate}
        [HttpPut("{AccountId}/deactivate/{IsDeactivate}")]
        [Authorize(Policy = "Admin.BasicAccess")]
        public async Task<IActionResult> DeactivateAccountById(int AccountId, bool IsDeactivate)
        {
            var requestData = JObject.FromObject(new
            {
                AccountId = AccountId
            });
            var deactivationFlowName = IsDeactivate ? "user-deactivation-flow" : "user-activation-flow";

            var startSagaTriggerMessage = _kafkaProducerService.PrepareStartSagaTriggerMessage("user-management-domain", requestData, null, deactivationFlowName);
            await _messagingService.SendSagaMessageAsync(startSagaTriggerMessage);
            return Ok(new
            {
                SagaInstanceId = startSagaTriggerMessage.SagaInstanceId
            }
            );
        }


        // /api/user-service/api/accounts/{AccountId}/violation-points/add
        [HttpPut("{AccountId}/violation-points/add")]
        [Authorize(Policy = "AdminOrStaff.BasicAccess")]


        //         // /api/user-service/api/auth/register/customer
        // [HttpPost("register/customer")]
        // public async Task<IActionResult> RegisterCustomer([FromForm] CustomerRegisterRequestDTO customerRegisterRequestDTO)
        // {
        //     var RegisterInfo = JsonConvert.DeserializeObject<CustomerRegisterInfoDTO>(customerRegisterRequestDTO.RegisterInfo);

        //     string mainImageFileKey = null;
        //     if (customerRegisterRequestDTO.MainImageFile != null)
        //     {
        //         // bool IsValidFile(string fieldName, string fileName, long fileSizeBytes, string mimeType);
        //         var isValidImage = _fileValidationConfig.IsValidFile("Account.mainImageFileKey", customerRegisterRequestDTO.MainImageFile.FileName, customerRegisterRequestDTO.MainImageFile.Length, customerRegisterRequestDTO.MainImageFile.ContentType);
        //         if (!isValidImage)
        //         {
        //             return BadRequest("Invalid image file.");
        //         }


        //         string newMainImageFileName = $"{Guid.NewGuid()}_{customerRegisterRequestDTO.MainImageFile.FileName}";
        //         using (var stream = customerRegisterRequestDTO.MainImageFile.OpenReadStream())
        //         {
        //             await _fileIOHelper.UploadBinaryFileWithStreamAsync(
        //                                 stream,
        //                                 _filePathConfig.ACCOUNT_TEMP_FILE_PATH,
        //                                 newMainImageFileName
        //                             );
        //         }

        //         mainImageFileKey = FilePathHelper.CombinePaths(_filePathConfig.ACCOUNT_TEMP_FILE_PATH, newMainImageFileName);

        //     }
        //     JObject requestData = JObject.FromObject(RegisterInfo);
        //     requestData["MainImageFileKey"] = mainImageFileKey;
        //     requestData["RoleId"] = 1;

        //     var startSagaTriggerMessage = _kafkaProducerService.PrepareStartSagaTriggerMessage("user-management-domain", requestData, null, "user-registration-flow");
        //     await _messagingService.SendSagaMessageAsync(startSagaTriggerMessage);
        //     return Ok(new
        //     {
        //         SagaInstanceId = startSagaTriggerMessage.SagaInstanceId
        //     }
        //     );
        // }


        // // /api/user-service/api/auth/account-verification
        // [HttpPost("account-verification")]
        // public async Task<IActionResult> AccountVerification([FromBody] AccountVerificationRequestDTO accountVerificationRequestDTO)
        // {
        //     // var accountVerificationInfo = JsonConvert.DeserializeObject<AccountVerificationInfoDTO>(accountVerificationRequestDTO.AccountVerificationInfo);
        //     var requestData = JObject.FromObject(accountVerificationRequestDTO.AccountVerificationInfo);
        //     var startSagaTriggerMessage = _kafkaProducerService.PrepareStartSagaTriggerMessage("user-management-domain", requestData, null, "user-email-verification-flow");
        //     await _messagingService.SendSagaMessageAsync(startSagaTriggerMessage);
        //     return Ok(new
        //     {
        //         SagaInstanceId = startSagaTriggerMessage.SagaInstanceId
        //     }
        //     );
        // }

        // // /api/user-service/api/auth/register/staff
        // [HttpPost("register/staff")]
        // public async Task<IActionResult> RegisterStaff([FromForm] StaffRegisterRequestDTO staffRegisterRequestDTO)
        // {
        //     var RegisterInfo = JsonConvert.DeserializeObject<StaffRegisterInfoDTO>(staffRegisterRequestDTO.RegisterInfo);

        //     string mainImageFileKey = null;
        //     if (staffRegisterRequestDTO.MainImageFile != null)
        //     {
        //         // bool IsValidFile(string fieldName, string fileName, long fileSizeBytes, string mimeType);
        //         var isValidImage = _fileValidationConfig.IsValidFile("Account.mainImageFileKey", staffRegisterRequestDTO.MainImageFile.FileName, staffRegisterRequestDTO.MainImageFile.Length, staffRegisterRequestDTO.MainImageFile.ContentType);
        //         if (!isValidImage)
        //         {
        //             return BadRequest("Invalid image file.");
        //         }

        //         string newMainImageFileName = $"{Guid.NewGuid()}_{staffRegisterRequestDTO.MainImageFile.FileName}";
        //         using (var stream = staffRegisterRequestDTO.MainImageFile.OpenReadStream())
        //         {
        //             await _fileIOHelper.UploadBinaryFileWithStreamAsync(
        //                                 stream,
        //                                 _filePathConfig.ACCOUNT_TEMP_FILE_PATH,
        //                                 newMainImageFileName
        //                             );
        //         }
        //         mainImageFileKey = FilePathHelper.CombinePaths(_filePathConfig.ACCOUNT_TEMP_FILE_PATH, newMainImageFileName);

        //     }
        //     JObject requestData = JObject.FromObject(RegisterInfo);
        //     requestData["MainImageFileKey"] = mainImageFileKey;
        //     requestData["RoleId"] = 2;

        //     var startSagaTriggerMessage = _kafkaProducerService.PrepareStartSagaTriggerMessage("user-management-domain", requestData, null, "user-registration-flow");
        //     await _messagingService.SendSagaMessageAsync(startSagaTriggerMessage);
        //     return Ok(new
        //     {
        //         SagaInstanceId = startSagaTriggerMessage.SagaInstanceId
        //     }
        //     );
        // }


        // // /api/user-service/api/auth/login-manual
        // [HttpPost("login-manual")]
        // public async Task<IActionResult> LoginManual([FromBody] ManualLoginRequestDTO manualLoginRequestDTO)
        // {
        //     var requestData = JObject.FromObject(manualLoginRequestDTO.ManualLoginInfo);
        //     var startSagaTriggerMessage = _kafkaProducerService.PrepareStartSagaTriggerMessage("user-management-domain", requestData, null, "user-manual-login-flow");
        //     await _messagingService.SendSagaMessageAsync(startSagaTriggerMessage);
        //     return Ok(new
        //     {
        //         SagaInstanceId = startSagaTriggerMessage.SagaInstanceId
        //     }
        //     );
        // }

        // // /api/user-service/api/auth/login-google
        // [HttpPost("login-google")]
        // public async Task<IActionResult> LoginGoogle([FromBody] GoogleLoginRequestDTO googleLoginRequestDTO)
        // {
        //     var requestData = JObject.FromObject(googleLoginRequestDTO.GoogleAuth);
        //     var startSagaTriggerMessage = _kafkaProducerService.PrepareStartSagaTriggerMessage("user-management-domain", requestData, null, "user-google-login-flow");
        //     await _messagingService.SendSagaMessageAsync(startSagaTriggerMessage);
        //     return Ok(new
        //     {
        //         SagaInstanceId = startSagaTriggerMessage.SagaInstanceId
        //     }
        //     );
        // }

        // // /api/user-service/api/auth/forgot-password
        // [HttpPost("forgot-password")]
        // public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestDTO forgotPasswordRequestDTO)
        // {
        //     var requestData = JObject.FromObject(forgotPasswordRequestDTO);
        //     var startSagaTriggerMessage = _kafkaProducerService.PrepareStartSagaTriggerMessage("user-management-domain", requestData, null, "forgot-password-flow");
        //     await _messagingService.SendSagaMessageAsync(startSagaTriggerMessage);
        //     return Ok(new
        //     {
        //         SagaInstanceId = startSagaTriggerMessage.SagaInstanceId
        //     }
        //     );
        // }

        // // /api/user-service/api/auth/new-reset-password
        // [HttpPost("new-reset-password")]
        // public async Task<IActionResult> NewResetPassword([FromBody] NewResetPasswordRequestDTO newResetPasswordRequestDTO)
        // {
        //     var requestData = JObject.FromObject(newResetPasswordRequestDTO.ResetPasswordInfo);
        //     var startSagaTriggerMessage = _kafkaProducerService.PrepareStartSagaTriggerMessage("user-management-domain", requestData, null, "password-reset-flow");
        //     await _messagingService.SendSagaMessageAsync(startSagaTriggerMessage);
        //     return Ok(new
        //     {
        //         SagaInstanceId = startSagaTriggerMessage.SagaInstanceId
        //     }
        //     );
        // }

    }
}
