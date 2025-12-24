namespace ModerationService.BusinessLogic.DTOs.SystemConfiguration
{
    public class SystemConfigProfileDTO
    {
        public int Id { get; set; }

        public string Name { get; set; } = null!;

        public bool IsActive { get; set; }

        public DateTime? DeletedAt { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        public AccountConfigDTO? AccountConfig { get; set; }

        public List<AccountViolationLevelConfigDTO> AccountViolationLevelConfigs { get; set; } = new List<AccountViolationLevelConfigDTO>();

        public BookingConfigDTO? BookingConfig { get; set; }

        public List<PodcastSubscriptionConfigDTO> PodcastSubscriptionConfigs { get; set; } = new List<PodcastSubscriptionConfigDTO>();
        public PodcastSuggestionConfigDTO? PodcastSuggestionConfig { get; set; }

        public ReviewSessionConfigDTO? ReviewSessionConfig { get; set; }
    }

}

