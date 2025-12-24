namespace TransactionService.BusinessLogic.DTOs.SystemConfiguration
{
    public class AccountConfigDTO
    {
        public int ConfigProfileId { get; set; }

        public int ViolationPointDecayHours { get; set; }

        public int PodcastListenSlotThreshold { get; set; }

        public int PodcastListenSlotRecoverySeconds { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

    }

}

