namespace PodcastService.BusinessLogic.DTOs.MessageQueue.ContentManagementDomain.CompleteAllUserEpisodeListenSessions
{
    public class CompleteAllUserEpisodeListenSessionsParameterDTO
    {
        public required int AccountId { get; set; }
        public required bool IsEpisodeListenSessionCompleted { get; set; }
    }
}
