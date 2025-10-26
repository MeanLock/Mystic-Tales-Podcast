namespace UserService.BusinessLogic.DTOs.MessageQueue.UserManagementDomain.SubtractAccountListenSlot
{
    public class SubtractAccountListenSlotParameterDTO
    {
        public required int AccountId { get; set; }
        public required int PodcastListenSlotAmount { get; set; }
    }
}
