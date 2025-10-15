namespace PodcastService.BusinessLogic.DTOs.MessageQueue.UserManagementDomain.AddAccountViolationPoint
{
    public class AddAccountViolationPointParameterDTO
    {
        public required int AccountId { get; set; }
        public required int ViolationPoint { get; set; }
    }
}
