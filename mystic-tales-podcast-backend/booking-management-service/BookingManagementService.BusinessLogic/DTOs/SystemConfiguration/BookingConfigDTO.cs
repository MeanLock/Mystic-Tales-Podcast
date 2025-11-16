namespace BookingManagementService.BusinessLogic.DTOs.SystemConfiguration
{
    public class BookingConfigDTO
    {
        public int ConfigProfileId { get; set; }

        public double ProfitRate { get; set; }

        public double DepositRate { get; set; }

        public int PodcastTrackPreviewListenSlot { get; set; }

        public int PreviewResponseAllowedDays { get; set; }

        public int ProducingRequestResponseAllowedDays { get; set; }

        public int ChatRoomExpiredHours { get; set; }

        public int ChatRoomFileMessageExpiredHours { get; set; }

        public double FreeInitialBookingStorageSize { get; set; }

        public decimal SingleStorageUnitPurchasePrice { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

    }

}

