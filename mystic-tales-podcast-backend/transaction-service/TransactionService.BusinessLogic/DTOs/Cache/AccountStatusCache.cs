using Newtonsoft.Json;
using TransactionService.BusinessLogic.Helpers.JsonHelpers;

namespace TransactionService.BusinessLogic.DTOs.Cache
{
    public class AccountStatusCache
    {
        public required int Id { get; set; }
        public required string Email { get; set; }
        public required string FullName { get; set; }
        public string? MainImageFileKey { get; set; }
        public int RoleId { get; set; }
        public bool IsVerified { get; set; }
        public int ViolationLevel { get; set; }
        public int ViolationPoint { get; set; }
        public string? PodcasterProfileName { get; set; }
        public bool? PodcasterProfileIsVerified { get; set; }
        public bool HasVerifiedPodcasterProfile { get; set; }


        [JsonConverter(typeof(NullableDateTimeJsonConverter))]
        public DateTime? LastViolationPointChanged { get; set; }

        [JsonConverter(typeof(NullableDateTimeJsonConverter))]
        public DateTime? LastViolationLevelChanged { get; set; }

        [JsonConverter(typeof(NullableDateTimeJsonConverter))]
        public DateTime? DeactivatedAt { get; set; }
    }


}