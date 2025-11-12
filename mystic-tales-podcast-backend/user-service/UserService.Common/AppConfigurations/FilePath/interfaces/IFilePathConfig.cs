namespace UserService.Common.AppConfigurations.FilePath.interfaces
{
    public interface IFilePathConfig
    {
        string ACCOUNT_FILE_PATH { get; set; }
        string ACCOUNT_TEMP_FILE_PATH { get; set; }
        string PODCAST_CHANNEL_FILE_PATH { get; set; }
        string PODCAST_CHANNEL_TEMP_FILE_PATH { get; set; }
        string PODCAST_SHOW_FILE_PATH { get; set; }
        string PODCAST_SHOW_TEMP_FILE_PATH { get; set; }
        string PODCAST_EPISODE_FILE_PATH { get; set; }
        string PODCAST_EPISODE_TEMP_FILE_PATH { get; set; }
        string BOOKING_FILE_PATH { get; set; }
        string BOOKING_TEMP_FILE_PATH { get; set; }
        string DMCA_ACCUSATION_FILE_PATH { get; set; }
        string DMCA_ACCUSATION_TEMP_FILE_PATH { get; set; }
        string ACCOUNT_BALANCE_WITHDRAWAL_REQUEST_FILE_PATH { get; set; }
        string ACCOUNT_BALANCE_WITHDRAWAL_REQUEST_TEMP_FILE_PATH { get; set; }
        // SystemDocuments
        string SYSTEM_PODCASTER_DOCUMENTS_FILE_PATH { get; set; }
    }
}
