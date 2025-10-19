namespace UserService.BusinessLogic.DTOs.MessageQueue.UserManagementDomain.CreateEpisodeSaved
{
    public class CreateEpisodeSavedParameterDTO
    {
        public required int AccountId { get; set; }
        public required Guid PodcastEpisodeId { get; set; }
    }
}
