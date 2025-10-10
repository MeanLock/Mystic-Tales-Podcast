using Newtonsoft.Json;
using SubscriptionService.BusinessLogic.Helpers.JsonHelpers;

namespace SubscriptionService.BusinessLogic.DTOs.Cache
{
    public class AccountStatusCache
    {
        public required int Id { get; set; }
        public int RoleId { get; set; }
        public bool IsVerified { get; set; }
        public int ViolationLevel { get; set; }
        public int ViolationPoint { get; set; }

        [JsonConverter(typeof(NullableDateTimeJsonConverter))]
        public DateTime? LastViolationPointChanged { get; set; }

        [JsonConverter(typeof(NullableDateTimeJsonConverter))]
        public DateTime? LastViolationLevelChanged { get; set; }

        [JsonConverter(typeof(NullableDateTimeJsonConverter))]
        public DateTime? DeactivatedAt { get; set; }
    }

}