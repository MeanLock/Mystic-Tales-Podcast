using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Client;
using Newtonsoft.Json.Linq;
using SubscriptionService.API.Filters.ExceptionFilters;
using SubscriptionService.BusinessLogic.DTOs.Cache;
using SubscriptionService.BusinessLogic.DTOs.PodcastSubscription;
using SubscriptionService.BusinessLogic.Models.CrossService;
using SubscriptionService.BusinessLogic.Services.CrossServiceServices.QueryServices;
using SubscriptionService.BusinessLogic.Services.DbServices.SubscriptionServices;
using SubscriptionService.BusinessLogic.Services.MessagingServices.interfaces;
using SubscriptionService.Infrastructure.Services.Kafka;

namespace SubscriptionService.API.Controllers.BaseControllers
{
    [Route("api/member-subscriptions")]
    [ApiController]
    [TypeFilter(typeof(HttpExceptionFilter))]
    [Authorize(Policy = "OptionalAccess")]
    public class MemberSubscriptionController : ControllerBase
    {
        private readonly GenericQueryService _genericQueryService;
        private readonly HttpServiceQueryClient _httpServiceQueryClient;
        private readonly MemberSubscriptionService _memberSubscriptionService;
        private readonly KafkaProducerService _kafkaProducerService;
        private readonly IMessagingService _messagingService;

        public MemberSubscriptionController(
            GenericQueryService genericQueryService, 
            HttpServiceQueryClient httpServiceQueryClient,
            MemberSubscriptionService memberSubscriptionService,
            KafkaProducerService kafkaProducerService,
            IMessagingService messagingService)
        {
            _genericQueryService = genericQueryService;
            _httpServiceQueryClient = httpServiceQueryClient;
            _memberSubscriptionService = memberSubscriptionService;
            _kafkaProducerService = kafkaProducerService;
            _messagingService = messagingService;
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
        [HttpGet]
        [Authorize(Policy = "BasicAccess")]
        public async Task<IActionResult> GetAllMemberSubscriptions()
        {
            var account = HttpContext.Items["LoggedInAccount"] as AccountStatusCache;
            var roleId = account.RoleId;

            var memberSubscriptions = await _memberSubscriptionService.GetAllMemberSubscriptionsAsync(roleId);
            return Ok(memberSubscriptions);
        }
        [HttpPost]
        [Authorize(Policy = "AdminOrStaff.BasicAccess")]
        public async Task<IActionResult> CreateMemberSubscription([FromBody] MemberSubscriptionCreateRequestDTO request)
        {
            var account = HttpContext.Items["LoggedInAccount"] as AccountStatusCache;
            var roleId = account.RoleId;

            var requestData = new JObject
            {
                { "Name", request.MemberSubscriptionCreateInfo.Name },
                { "Description", request.MemberSubscriptionCreateInfo.Description },
                { "PodcastSubscriptionCycleTypePriceList", JArray.FromObject(request.MemberSubscriptionCreateInfo.MemberSubscriptionCycleTypePriceCreateInfoList) },
                { "PodcastSubscriptionBenefitMappingList", JArray.FromObject(request.MemberSubscriptionCreateInfo.MemberSubscriptionBenefitMappingCreateInfoList) }
            };
            var startSagaTriggerMessage = _kafkaProducerService.PrepareStartSagaTriggerMessage(
                topic: "subscription-management-domain",
                requestData: requestData,
                sagaInstanceId: null,
                messageName: "member-subscription-creation-flow");
            var result = await _messagingService.SendSagaMessageAsync(startSagaTriggerMessage);
            if (!result)
            {
                return StatusCode(500, "Failed to initiate member subscription creation.");
            }
            return Ok(new
            {
                SagaInstanceId = startSagaTriggerMessage.SagaInstanceId
            });
        }
        [HttpGet("{MemberSubscriptionId}")]
        [Authorize(Policy = "BasicAccess")]
        public async Task<IActionResult> GetMemberSubscriptionByIdAsync([FromRoute] int MemberSubscriptionId)
        {
            var memberSubscriptions = await _memberSubscriptionService.GetMemberSubscriptionIdAsync(MemberSubscriptionId);
            return Ok(memberSubscriptions);
        }
    }
}
