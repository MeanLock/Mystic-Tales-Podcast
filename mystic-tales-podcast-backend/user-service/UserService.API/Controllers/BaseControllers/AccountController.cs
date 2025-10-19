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
    [Authorize(Policy = "OptionalAccess")]
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
        private readonly HttpServiceQueryClient _httpServiceQueryClient;

        public AccountController(FileIOHelper fileIOHelper, KafkaProducerService kafkaProducerService, IMessagingService messagingService, IFileValidationConfig fileValidationConfig, IFilePathConfig filePathConfig, AccountService accountService, RedisInstanceCacheService redisInstanceCacheService, RedisSharedCacheService redisSharedCacheService, HttpServiceQueryClient httpServiceQueryClient)
        {
            _fileIOHelper = fileIOHelper;
            _kafkaProducerService = kafkaProducerService;
            _messagingService = messagingService;
            _fileValidationConfig = fileValidationConfig;
            _accountService = accountService;
            _filePathConfig = filePathConfig;
            _redisInstanceCacheService = redisInstanceCacheService;
            _redisSharedCacheService = redisSharedCacheService;
            _httpServiceQueryClient = httpServiceQueryClient;
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

        [HttpGet("test-delete-account-status-cache/{accountId}")]
        public async Task<IActionResult> TestDeleteAccountStatusCache(int accountId)
        {
            await _redisSharedCacheService.KeyDeleteAsync($"account:status:{accountId}");
            return Ok(new { Message = $"Deleted account status cache for account ID: {accountId}" });
        }

        [HttpGet("test-query")]
        public async Task<IActionResult> TestQuery()
        {
            var podcastChannelBatchRequest = new BatchQueryRequest
            {
                Queries = new List<BatchQueryItem>
                    {
                        new BatchQueryItem
                        {
                            Key = "podcastShow",
                            QueryType = "findbyid",
                            EntityType = "PodcastShow",
                            Parameters = JObject.FromObject(new
                            {
                                where =  new {
                                    DeletedAt = (DateTime?)null,
                                },
                                id = "172eb07f-2121-4ff7-8b5c-91eeec0dee86",
                                include = "PodcastShowStatusTrackings"
                            }),
                        }
                    }
            };

            var result = await _httpServiceQueryClient.ExecuteBatchAsync("PodcastService", podcastChannelBatchRequest);
            return Ok(new { result });
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
        // [Authorize(Policy = "AdminOrStaffOrCustomer.BasicAccess")]
        public async Task<IActionResult> GetPodcastBuddies()
        {
            var account = HttpContext.Items["LoggedInAccount"] as AccountStatusCache;
            Console.WriteLine($"Logged in account from HttpContext.Items: Id={account?.Id}, RoleId={account?.RoleId}, IsVerified={account?.IsVerified}, DeactivatedAt={account?.DeactivatedAt}");
            var podcastBuddies = await _accountService.GetPodcastBuddyAccounts(account?.RoleId);

            return Ok(new { PodcastBuddyList = podcastBuddies });
        }

        // /api/user-service/api/accounts/podcaster/apply
        [HttpPost("podcaster/apply")]
        [Authorize(Policy = "Customer.NoViolationAccess.NonPodcasterAccess")]
        public async Task<IActionResult> ApplyPodcaster([FromForm] PodcasterProfileCreateRequestDTO podcasterProfileRequestDTO)
        {
            var account = HttpContext.Items["LoggedInAccount"] as AccountStatusCache;

            var podcasterProfileRequestInfo = JsonConvert.DeserializeObject<PodcasterProfileCreateInfoDTO>(podcasterProfileRequestDTO.PodcasterProfileCreateInfo);

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

            var podcasterProfileRequestInfo = JsonConvert.DeserializeObject<PodcasterProfileUpdateInfoDTO>(podcasterProfileUpdateRequestDTO.PodcasterProfileUpdateInfo);

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
        [Authorize(Policy = "Admin.BasicAccess")]
        public async Task<IActionResult> AddViolationPointsToAccountById(ViolationPointChangeRequestDTO violationPointChangeRequestDTO, int AccountId)
        {
            var requestData = JObject.FromObject(new
            {
                AccountId = AccountId,
                ViolationPoint = violationPointChangeRequestDTO.ViolationPoint
            });

            var startSagaTriggerMessage = _kafkaProducerService.PrepareStartSagaTriggerMessage("user-management-domain", requestData, null, "user-violation-punishment-flow");
            await _messagingService.SendSagaMessageAsync(startSagaTriggerMessage);
            return Ok(new
            {
                SagaInstanceId = startSagaTriggerMessage.SagaInstanceId
            }
            );
        }


        // /api/user-service/api/accounts/podcasters/{AccountId}/verify/{IsVerify}
        [HttpPut("podcasters/{AccountId}/verify/{IsVerify}")]
        [Authorize(Policy = "AdminOrStaff.BasicAccess")]
        public async Task<IActionResult> VerifyPodcasterById(int AccountId, bool IsVerify)
        {
            var requestData = JObject.FromObject(new
            {
                AccountId = AccountId,
                IsVerified = IsVerify
            });

            var startSagaTriggerMessage = _kafkaProducerService.PrepareStartSagaTriggerMessage("user-management-domain", requestData, null, "podcaster-verification-flow");
            await _messagingService.SendSagaMessageAsync(startSagaTriggerMessage);
            return Ok(new
            {
                SagaInstanceId = startSagaTriggerMessage.SagaInstanceId
            }
            );
        }

        // /api/user-service/api/accounts/{AccountId}/podcast-buddy-reviews
        [HttpPost("{AccountId}/podcast-buddy-reviews")]
        [Authorize(Policy = "Customer.NoViolationAccess")]
        public async Task<IActionResult> GetPodcastBuddyReviewsByAccountId(PodcastBuddyReviewCreateRequestDTO podcastBuddyReviewCreateRequestDTO, int AccountId)
        {
            var account = HttpContext.Items["LoggedInAccount"] as AccountStatusCache;
            if (account.Id == AccountId)
            {
                return StatusCode(403, "You cannot review yourself as a podcast buddy.");
            }
            if (podcastBuddyReviewCreateRequestDTO.PodcastBuddyReviewCreateInfo.Rating < 0 || podcastBuddyReviewCreateRequestDTO.PodcastBuddyReviewCreateInfo.Rating > 5)
            {
                return BadRequest("Rating must be between 0 and 5.");
            }
            var requestData = JObject.FromObject(new
            {
                AccountId = account.Id,
                PodcastBuddyId = AccountId,
                Title = podcastBuddyReviewCreateRequestDTO.PodcastBuddyReviewCreateInfo.Title,
                Content = podcastBuddyReviewCreateRequestDTO.PodcastBuddyReviewCreateInfo.Content,
                Rating = podcastBuddyReviewCreateRequestDTO.PodcastBuddyReviewCreateInfo.Rating
            });

            var startSagaTriggerMessage = _kafkaProducerService.PrepareStartSagaTriggerMessage("public-review-management-domain", requestData, null, "podcast-buddy-review-creation-flow");
            await _messagingService.SendSagaMessageAsync(startSagaTriggerMessage);
            return Ok(new
            {
                SagaInstanceId = startSagaTriggerMessage.SagaInstanceId
            }
            );
        }

        // /api/user-service/api/accounts/podcast-buddy-reviews/{PodcastBuddyReviewId}
        [HttpPut("podcast-buddy-reviews/{PodcastBuddyReviewId}")]
        [Authorize(Policy = "Customer.NoViolationAccess")]
        public async Task<IActionResult> UpdatePodcastBuddyReviewById(PodcastBuddyReviewUpdateRequestDTO podcastBuddyReviewUpdateRequestDTO, Guid PodcastBuddyReviewId)
        {
            var account = HttpContext.Items["LoggedInAccount"] as AccountStatusCache;
            if (podcastBuddyReviewUpdateRequestDTO.PodcastBuddyReviewUpdateInfo.Rating < 0 || podcastBuddyReviewUpdateRequestDTO.PodcastBuddyReviewUpdateInfo.Rating > 5)
            {
                return BadRequest("Rating must be between 0 and 5.");
            }

            var requestData = JObject.FromObject(new
            {
                PodcastBuddyReviewId = PodcastBuddyReviewId,
                AccountId = account.Id,
                Title = podcastBuddyReviewUpdateRequestDTO.PodcastBuddyReviewUpdateInfo.Title,
                Content = podcastBuddyReviewUpdateRequestDTO.PodcastBuddyReviewUpdateInfo.Content,
                Rating = podcastBuddyReviewUpdateRequestDTO.PodcastBuddyReviewUpdateInfo.Rating
            });

            var startSagaTriggerMessage = _kafkaProducerService.PrepareStartSagaTriggerMessage("public-review-management-domain", requestData, null, "podcast-buddy-review-update-flow");
            await _messagingService.SendSagaMessageAsync(startSagaTriggerMessage);
            return Ok(new
            {
                SagaInstanceId = startSagaTriggerMessage.SagaInstanceId
            }
            );
        }

        // /api/user-service/api/accounts/podcast-buddy-reviews/{PodcastBuddyReviewId}
        [HttpDelete("podcast-buddy-reviews/{PodcastBuddyReviewId}")]
        [Authorize(Policy = "Customer.NoViolationAccess")]
        public async Task<IActionResult> DeletePodcastBuddyReviewById(Guid PodcastBuddyReviewId)
        {
            var account = HttpContext.Items["LoggedInAccount"] as AccountStatusCache;
            var requestData = JObject.FromObject(new
            {
                PodcastBuddyReviewId = PodcastBuddyReviewId,
                AccountId = account.Id
            });

            var startSagaTriggerMessage = _kafkaProducerService.PrepareStartSagaTriggerMessage("public-review-management-domain", requestData, null, "podcast-buddy-review-deletion-flow");
            await _messagingService.SendSagaMessageAsync(startSagaTriggerMessage);
            return Ok(new
            {
                SagaInstanceId = startSagaTriggerMessage.SagaInstanceId
            }
            );
        }

        // /api/user-service/api/accounts/{AccountId}/{IsFollow}
        [HttpPost("{AccountId}/follow/{IsFollow}")]
        [Authorize(Policy = "Customer.BasicAccess")]
        public async Task<IActionResult> FollowOrUnfollowPodcaster(int AccountId, bool IsFollow)
        {
            var account = HttpContext.Items["LoggedInAccount"] as AccountStatusCache;
            if (account.Id == AccountId)
            {
                return StatusCode(403, "You cannot follow/unfollow yourself.");
            }
            var requestData = JObject.FromObject(new
            {
                AccountId = account.Id,
                PodcastBuddyId = AccountId,
            });
            var flowName = IsFollow ? "podcaster-follow-flow" : "podcaster-unfollow-flow";

            var startSagaTriggerMessage = _kafkaProducerService.PrepareStartSagaTriggerMessage("user-management-domain", requestData, null, flowName);
            await _messagingService.SendSagaMessageAsync(startSagaTriggerMessage);
            return Ok(new
            {
                SagaInstanceId = startSagaTriggerMessage.SagaInstanceId
            }
            );
        }




    }
}
