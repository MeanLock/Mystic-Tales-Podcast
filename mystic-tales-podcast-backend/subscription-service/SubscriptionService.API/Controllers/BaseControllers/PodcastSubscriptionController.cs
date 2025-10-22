using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using SubscriptionService.API.Filters.ExceptionFilters;
using SubscriptionService.BusinessLogic.DTOs.Cache;
using SubscriptionService.BusinessLogic.DTOs.PodcastSubscription;
using SubscriptionService.BusinessLogic.Enums.Kafka;
using SubscriptionService.BusinessLogic.Models.CrossService;
using SubscriptionService.BusinessLogic.Services.CrossServiceServices.QueryServices;
using SubscriptionService.BusinessLogic.Services.DbServices.SubscriptionServices;
using SubscriptionService.BusinessLogic.Services.MessagingServices.interfaces;
using SubscriptionService.Infrastructure.Services.Kafka;

namespace SubscriptionService.API.Controllers.BaseControllers
{
    [Route("api/podcast-subscriptions")]
    [ApiController]
    [TypeFilter(typeof(HttpExceptionFilter))]
    [Authorize(Policy = "OptionalAccess")]
    public class PodcastSubscriptionController : ControllerBase
    {
        private readonly GenericQueryService _genericQueryService;
        private readonly HttpServiceQueryClient _httpServiceQueryClient;
        private readonly PodcastSubscriptionService _podcastSubscriptionService;
        private readonly ILogger<PodcastSubscriptionController> _logger;
        private readonly KafkaProducerService _kafkaProducerService;
        private readonly IMessagingService _messagingService;
        private const string SAGA_TOPIC = KafkaTopicEnum.SubscriptionManagementDomain;

        public PodcastSubscriptionController(
            GenericQueryService genericQueryService,
            HttpServiceQueryClient httpServiceQueryClient,
            PodcastSubscriptionService podcastSubscriptionService,
            ILogger<PodcastSubscriptionController> logger,
            KafkaProducerService kafkaProducerService,
            IMessagingService messagingService)
        {
            _genericQueryService = genericQueryService;
            _httpServiceQueryClient = httpServiceQueryClient;
            _podcastSubscriptionService = podcastSubscriptionService;
            _logger = logger;
            _kafkaProducerService = kafkaProducerService;
            _messagingService = messagingService;
        }
        [HttpGet("shows/{PodcastShowId}")]
        [Authorize(Policy = "Customer.NoViolationAccess.PodcasterAccess")]
        public async Task<IActionResult> GetPodcastSubscriptionByPodcastShowId([FromRoute] Guid PodcastShowId)
        {
            var account = HttpContext.Items["LoggedInAccount"] as AccountStatusCache;
            var accountId = account.Id;

            var isValid = await _podcastSubscriptionService.GetPodcastShowWithAccountId(accountId, PodcastShowId);
            if (isValid == null)
            {
                return Forbid($"The Logged In Account is unauthorized to access Podcast Show Id: {PodcastShowId}");
            }
            var podcastSubscription = await _podcastSubscriptionService.GetPodcastSubscriptionListByPodcastShowIdAsync(PodcastShowId);
            //if (podcastSubscription == null)
            //{
            //    return NotFound($"No podcast subscription found with Show Id: {PodcastShowId}");
            //}
            return Ok(new
            {
                ShowSubscriptionList = podcastSubscription
            });
        }
        [HttpPost("shows/{PodcastShowId}")]
        [Authorize(Policy = "Customer.NoViolationAccess.PodcasterAccess")]
        public async Task<IActionResult> CreatePodcastSubscription(
            [FromRoute] Guid PodcastShowId,
            [FromBody] PodcastSubscriptionCreateRequestDTO request)
        {
            var account = HttpContext.Items["LoggedInAccount"] as AccountStatusCache;
            var accountId = account.Id;

            var requestData = new JObject
            {
                { "AccountId", accountId },
                { "Name", request.PodcastSubscriptionCreateInfo.Name },
                { "Description", request.PodcastSubscriptionCreateInfo.Description },
                { "PodcastShowId", PodcastShowId  },
                { "PodcastSubscriptionCycleTypePriceList", JArray.FromObject(request.PodcastSubscriptionCreateInfo.PodcastSubscriptionCycleTypePriceCreateInfoList) },
                { "PodcastSubscriptionBenefitMappingList", JArray.FromObject(request.PodcastSubscriptionCreateInfo.PodcastSubscriptionBenefitMappingCreateInfoList) }
            };
            var startSagaTriggerMessage = _kafkaProducerService.PrepareStartSagaTriggerMessage(
                topic: SAGA_TOPIC,
                requestData: requestData,
                sagaInstanceId: null,
                messageName: "podcast-subscription-creation-flow");
            var result = await _messagingService.SendSagaMessageAsync(startSagaTriggerMessage);
            if (!result)
            {
                return StatusCode(500, "Failed to initiate podcast subscription creation.");
            }
            return Ok(new
            {
                SagaInstanceId = startSagaTriggerMessage.SagaInstanceId
            });
        }
        [HttpGet("channels/{PodcastChannelId}")]
        [Authorize(Policy = "Customer.NoViolationAccess.PodcasterAccess")]
        public async Task<IActionResult> GetPodcastSubscriptionByPodcastChannelId([FromRoute] Guid PodcastChannelId)
        {
            var account = HttpContext.Items["LoggedInAccount"] as AccountStatusCache;
            var accountId = account.Id;

            var isValid = await _podcastSubscriptionService.GetPodcastChannelWithAccountId(accountId, PodcastChannelId);
            if (isValid == null)
            {
                return Forbid($"The Logged In Account is unauthorized to access Podcast Channel Id: {PodcastChannelId}");
            }
            var podcastSubscription = await _podcastSubscriptionService.GetPodcastSubscriptionListByPodcastChannelIdAsync(PodcastChannelId);
            //if (podcastSubscription == null)
            //{
            //    return NotFound($"No podcast subscription found with Channel Id: {PodcastChannelId}");
            //}
            return Ok(new
            {
                PodcastSubscriptionList = podcastSubscription
            });
        }
        [HttpPost("channels/{PodcastChannelId}")]
        [Authorize(Policy = "Customer.NoViolationAccess.PodcasterAccess")]
        public async Task<IActionResult> CreatePodcastSubscriptionByChannelId(
            [FromRoute] Guid PodcastChannelId,
            [FromBody] PodcastSubscriptionCreateRequestDTO request)
        {
            var account = HttpContext.Items["LoggedInAccount"] as AccountStatusCache;
            var accountId = account.Id;
            //var isValid = await _podcastSubscriptionService.GetPodcastChannel(accountId, PodcastChannelId);
            //if (isValid == null)
            //{
            //    return Forbid($"The Logged In Account is unauthorized to create PodcastSubscription for PodcastChannel with Id: {PodcastChannelId}");
            //}
            var requestData = new JObject
            {
                { "AccountId", accountId },
                { "Name", request.PodcastSubscriptionCreateInfo.Name },
                { "Description", request.PodcastSubscriptionCreateInfo.Description },
                { "PodcastChannelId", PodcastChannelId  },
                { "PodcastSubscriptionCycleTypePriceList", JArray.FromObject(request.PodcastSubscriptionCreateInfo.PodcastSubscriptionCycleTypePriceCreateInfoList) },
                { "PodcastSubscriptionBenefitMappingList", JArray.FromObject(request.PodcastSubscriptionCreateInfo.PodcastSubscriptionBenefitMappingCreateInfoList) }
            };
            var startSagaTriggerMessage = _kafkaProducerService.PrepareStartSagaTriggerMessage(
                topic: SAGA_TOPIC,
                requestData: requestData,
                sagaInstanceId: null,
                messageName: "podcast-subscription-creation-flow");
            var result = await _messagingService.SendSagaMessageAsync(startSagaTriggerMessage);
            if (!result)
            {
                return StatusCode(500, "Failed to initiate podcast subscription creation.");
            }
            return Ok(new
            {
                SagaInstanceId = startSagaTriggerMessage.SagaInstanceId
            });
        }
        [HttpGet("{PodcastSubscriptionId}")]
        [Authorize(Policy = "Customer.NoViolationAccess.PodcasterAccess")]
        public async Task<IActionResult> GetPodcastSubscriptionById([FromRoute] int PodcastSubscriptionId)
        {
            var account = HttpContext.Items["LoggedInAccount"] as AccountStatusCache;
            var accountId = account.Id;
            var isValid = await _podcastSubscriptionService.ValidatePodcastSubscriptionAccess(accountId, PodcastSubscriptionId);
            if (isValid == null)
            {
                return Forbid($"The Logged In Account is unauthorized to access Podcast Subscription Id: {PodcastSubscriptionId}");
            }

            var podcastSubscription = await _podcastSubscriptionService.GetPodcastSubscriptionByIdAsync(PodcastSubscriptionId);
            //if (podcastSubscription == null)
            //{
            //    return NotFound($"No podcast subscription found with Id: {PodcastSubscriptionId}");
            //}

            return Ok(new
            {
                PodcastSubscription = podcastSubscription
            });
        }
        [HttpPut("{PodcastSubscriptionId}")]
        [Authorize(Policy = "Customer.NoViolationAccess.PodcasterAccess")]
        public async Task<IActionResult> UpdatePodcastSubscriptionById(
            [FromRoute] int PodcastSubscriptionId,
            [FromBody] PodcastSubscriptionUpdateRequestDTO request)
        {
            var account = HttpContext.Items["LoggedInAccount"] as AccountStatusCache;
            var accountId = account.Id;
            //var isValid = await _podcastSubscriptionService.ValidatePodcastSubscriptionAccess(accountId, PodcastSubscriptionId);
            //if (isValid == null)
            //{
            //    return Forbid($"The Logged In Account is unauthorized to update Podcast Subscription Id: {PodcastSubscriptionId}");
            //}
            var requestData = new JObject
            {
                { "AccountId", accountId },
                { "Name", request.PodcastSubscriptionUpdateInfo.Name },
                { "Description", request.PodcastSubscriptionUpdateInfo.Description },
                { "PodcastSubscriptionId", PodcastSubscriptionId  },
                { "PodcastSubscriptionCycleTypePriceList", JArray.FromObject(request.PodcastSubscriptionUpdateInfo.PodcastSubscriptionCycleTypePriceUpdateInfoList) },
                { "PodcastSubscriptionBenefitMappingList", JArray.FromObject(request.PodcastSubscriptionUpdateInfo.PodcastSubscriptionBenefitMappingUpdateInfoList) }
            };
            var startSagaTriggerMessage = _kafkaProducerService.PrepareStartSagaTriggerMessage(
                topic: SAGA_TOPIC,
                requestData: requestData,
                sagaInstanceId: null,
                messageName: "podcast-subscription-update-flow");
            var result = await _messagingService.SendSagaMessageAsync(startSagaTriggerMessage);
            if (!result)
            {
                return StatusCode(500, "Failed to initiate podcast subscription updating.");
            }
            return Ok(new
            {
                SagaInstanceId = startSagaTriggerMessage.SagaInstanceId
            });
        }
        [HttpDelete("{PodcastSubscriptionId}")]
        [Authorize(Policy = "Customer.NoViolationAccess.PodcasterAccess")]
        public async Task<IActionResult> DeletePodcastSubscriptionById([FromRoute] int PodcastSubscriptionId)
        {
            var account = HttpContext.Items["LoggedInAccount"] as AccountStatusCache;
            var accountId = account.Id;
            //var isValid = await _podcastSubscriptionService.ValidatePodcastSubscriptionAccess(accountId, PodcastSubscriptionId);
            //if (isValid == null)
            //{
            //    return Forbid($"The Logged In Account is unauthorized to delete Podcast Subscription Id: {PodcastSubscriptionId}");
            //}

            var requestData = new JObject
            {
                { "AccountId", accountId },
                { "PodcastSubscriptionId", PodcastSubscriptionId }
            };
            var startSagaTriggerMessage = _kafkaProducerService.PrepareStartSagaTriggerMessage(
                topic: SAGA_TOPIC,
                requestData: requestData,
                sagaInstanceId: null,
                messageName: "podcast-subscription-deletion-flow");
            var result = await _messagingService.SendSagaMessageAsync(startSagaTriggerMessage);
            if (!result)
            {
                return StatusCode(500, "Failed to initiate podcast subscription deletion.");
            }
            return Ok(new
            {
                SagaInstanceId = startSagaTriggerMessage.SagaInstanceId
            });
        }
        [HttpPost("{PodcastSubscriptionId}")]
        [Authorize(Policy = "Customer.BasicAccess")]
        public async Task<IActionResult> SubscribeToPodcastSubscriptionById(
            [FromRoute] int PodcastSubscriptionId,
            [FromBody] PodcastSubscriptionRegistrationCreateRequestDTO request)
        {
            var account = HttpContext.Items["LoggedInAccount"] as AccountStatusCache;
            var accountId = account.Id;
            var podcastSubscription = await _podcastSubscriptionService.GetPodcastSubscriptionByIdAsync(PodcastSubscriptionId);
            if (podcastSubscription == null)
            {
                return NotFound($"No podcast subscription found with Id: {PodcastSubscriptionId}");
            }
            var requestData = new JObject
            {
                { "AccountId", accountId },
                { "PodcastSubscriptionId", PodcastSubscriptionId },
                { "SubscriptionCycleTypeId", request.SubscriptionCycleTypeId }
            };
            var startSagaTriggerMessage = _kafkaProducerService.PrepareStartSagaTriggerMessage(
                topic: SAGA_TOPIC,
                requestData: requestData,
                sagaInstanceId: null,
                messageName: "podcast-subscription-registration-flow");
            var result = await _messagingService.SendSagaMessageAsync(startSagaTriggerMessage);
            if (!result)
            {
                return StatusCode(500, "Failed to initiate podcast subscription process.");
            }
            return Ok(new
            {
                SagaInstanceId = startSagaTriggerMessage.SagaInstanceId
            });
        }
        [HttpGet("channels/podcast-subscriptions-registrations")]
        [Authorize(Policy = "Customer.BasicAccess")]
        public async Task<IActionResult> GetPodcastChannelSubscriptionsRegistrationsByAccountId()
        {
            var account = HttpContext.Items["LoggedInAccount"] as AccountStatusCache;
            var accountId = account.Id;

            var podcastSubscriptionsRegistrations = await _podcastSubscriptionService.GetChannelPodcastSubscriptionsRegistrationsByAccountIdAsync(accountId);
            return Ok(new
            {
                ChannelSubscriptionRegistrationList = podcastSubscriptionsRegistrations
            });
        }
        [HttpGet("shows/podcast-subscriptions-registrations")]
        [Authorize(Policy = "Customer.BasicAccess")]
        public async Task<IActionResult> GetPodcastShowSubscriptionsRegistrationsByAccountId()
        {
            var account = HttpContext.Items["LoggedInAccount"] as AccountStatusCache;
            var accountId = account.Id;
            var podcastSubscriptionsRegistrations = await _podcastSubscriptionService.GetShowPodcastSubscriptionsRegistrationsByAccountIdAsync(accountId);
            return Ok(new
            {
                ShowSubscriptionRegistrationList = podcastSubscriptionsRegistrations
            });
        }
        [HttpGet("podcast-subscriptions-registrations/{PodcastSubscriptionRegistrationId}")]
        [Authorize(Policy = "Customer.BasicAccess")]
        public async Task<IActionResult> GetPodcastSubscriptionRegistrationById([FromRoute] Guid PodcastSubscriptionRegistrationId)
        {
            var account = HttpContext.Items["LoggedInAccount"] as AccountStatusCache;
            var accountId = account.Id;
            var isValid = await _podcastSubscriptionService.ValidatePodcastSubscriptionRegistration(accountId, PodcastSubscriptionRegistrationId);
            if (isValid == null)
            {
                return Forbid($"The Logged In Account is unauthorized to access Podcast Subscription Registration Id: {PodcastSubscriptionRegistrationId}");
            }
            var podcastSubscriptionRegistration = await _podcastSubscriptionService.GetPodcastSubscriptionRegistrationByIdAsync(PodcastSubscriptionRegistrationId);
            //if (podcastSubscriptionRegistration == null)
            //{
            //    return NotFound($"No podcast subscription registration found with Id: {PodcastSubscriptionRegistrationId}");
            //}
            return Ok(new
            {
                PodcastSubscriptionRegistration = podcastSubscriptionRegistration
            });
        }
        [HttpPut("{PodcastSubscriptionId}/active/{IsActive}")]
        [Authorize(Policy = "Customer.NoViolationAccess.PodcasterAccess")]
        public async Task<IActionResult> UpdatePodcastSubscriptionActiveStatusById(
            [FromRoute] int PodcastSubscriptionId,
            [FromRoute] bool IsActive)
        {
            var account = HttpContext.Items["LoggedInAccount"] as AccountStatusCache;
            var accountId = account.Id;
            //var isValid = await _podcastSubscriptionService.ValidatePodcastSubscriptionAccess(accountId, PodcastSubscriptionId);
            //if (isValid == null)
            //{
            //    return Forbid($"The Logged In Account is unauthorized to update Podcast Subscription Id: {PodcastSubscriptionId}");
            //}

            var messageName = "";
            if (IsActive) messageName = "podcast-subscription-activation-flow";
            else messageName = "podcast-subscription-deactivation-flow";

            var requestData = new JObject
            {
                { "AccountId", accountId },
                { "PodcastSubscriptionId", PodcastSubscriptionId }
            };
            var startSagaTriggerMessage = _kafkaProducerService.PrepareStartSagaTriggerMessage(
                topic: SAGA_TOPIC,
                requestData: requestData,
                sagaInstanceId: null,
                messageName: messageName);
            var result = await _messagingService.SendSagaMessageAsync(startSagaTriggerMessage);
            if (!result)
            {
                return StatusCode(500, "Failed to initiate podcast subscription process.");
            }
            return Ok(new
            {
                SagaInstanceId = startSagaTriggerMessage.SagaInstanceId
            });
        }
        [HttpPut("podcast-subscriptions-registrations/{PodcastSubscriptionRegistrationId}/cancel")]
        [Authorize(Policy = "Customer.BasicAccess")]
        public async Task<IActionResult> CancelPodcastSubscriptionRegistrationById(
            [FromRoute] Guid PodcastSubscriptionRegistrationId)
        {
            var account = HttpContext.Items["LoggedInAccount"] as AccountStatusCache;
            var accountId = account.Id;

            var requestData = new JObject
            {
                { "AccountId", accountId },
                { "PodcastSubscriptionRegistrationId", PodcastSubscriptionRegistrationId }
            };
            var startSagaTriggerMessage = _kafkaProducerService.PrepareStartSagaTriggerMessage(
                topic: SAGA_TOPIC,
                requestData: requestData,
                sagaInstanceId: null,
                messageName: "podcast-subscription-cancellation-flow");
            var result = await _messagingService.SendSagaMessageAsync(startSagaTriggerMessage);
            if (!result)
            {
                return StatusCode(500, "Failed to initiate podcast subscription cancellation.");
            }
            return Ok(new
            {
                SagaInstanceId = startSagaTriggerMessage.SagaInstanceId
            });
        }
    }
}
