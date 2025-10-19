namespace UserService.BusinessLogic.DTOs.MessageQueue.UserManagementDomain.CreateEpisodeSavedRollback
{
    public class CreateEpisodeSavedRollbackParameterDTO
    {
        public required int AccountId { get; set; }
        public required Guid PodcastEpisodeId { get; set; }
    }
}
