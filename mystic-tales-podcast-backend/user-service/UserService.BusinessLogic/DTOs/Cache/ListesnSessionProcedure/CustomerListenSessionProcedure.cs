namespace UserService.BusinessLogic.DTOs.Cache.ListesnSessionProcedure
{

    public class CustomerListenSessionProcedure
    {
        public required Guid Id { get; set; }
        public required string PlayOrderMode { get; set; }
        public required bool IsAutoPlay { get; set; }
        public required ListenSessionProcedureSourceDetail SourceDetail { get; set; }
        public required List<ListenSessionProcedureListenObjectQueueItem> ListenObjectsSequentialOrder { get; set; }
        public required List<ListenSessionProcedureListenObjectQueueItem> ListenObjectsRandomOrder { get; set; }
        public required bool IsCompleted { get; set; }
        public required DateTime CreatedAt { get; set; }
    }

    public class ListenSessionProcedureSourceDetail
    {
        public required string Type { get; set; }
        public PodcastShowInfo? PodcastShow { get; set; }
        public BookingInfo? Booking { get; set; }
    }

    public class PodcastShowInfo
    {
        public required Guid Id { get; set; }
        public required string Name { get; set; }
    }

    public class BookingInfo
    {
        public required Guid BookingProducingRequestId { get; set; }
        public required string BookingProducingRequestTitle { get; set; }
    }

    public class ListenSessionProcedureListenObjectQueueItem
    {
        public required Guid ListenObjectId { get; set; }
        public required int Order { get; set; }
        public required bool IsListenable { get; set; }
    }


}