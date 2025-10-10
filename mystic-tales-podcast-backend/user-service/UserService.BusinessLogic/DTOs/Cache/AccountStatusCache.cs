using Newtonsoft.Json;
using UserService.BusinessLogic.Helpers.JsonHelpers;

namespace UserService.BusinessLogic.DTOs.Cache
{
    public class AccountStatusCache
    {
        public required int Id { get; set; }
        public int RoleId { get; set; }
        public bool IsVerified { get; set; }
        public int ViolationLevel { get; set; }
        public int ViolationPoint { get; set; }
        public bool HasVerifiedPodcasterProfile { get; set; }

        [JsonConverter(typeof(NullableDateTimeJsonConverter))]
        public DateTime? LastViolationPointChanged { get; set; }

        [JsonConverter(typeof(NullableDateTimeJsonConverter))]
        public DateTime? LastViolationLevelChanged { get; set; }

        [JsonConverter(typeof(NullableDateTimeJsonConverter))]
        public DateTime? DeactivatedAt { get; set; }
    }

}