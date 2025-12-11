namespace PodcastService.Common.AppConfigurations.FilePath.interfaces
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
        string PODCAST_BACKGROUND_SOUND_TRACK_FILE_PATH { get; set; }
        string PODCAST_BACKGROUND_SOUND_TRACK_TEMP_FILE_PATH { get; set; }
        string BOOKING_FILE_PATH { get; set; }
        string BOOKING_TEMP_FILE_PATH { get; set; }
        string DMCA_ACCUSATION_FILE_PATH { get; set; }
        string DMCA_ACCUSATION_TEMP_FILE_PATH { get; set; }
        string PODCAST_CATEGORY_FILE_PATH { get; set; }
        string PODCAST_CATEGORY_TEMP_FILE_PATH { get; set; }
        string HLS_PROCESSING_LOCAL_TEMP_FILE_PATH { get; set; }
        string AUDIO_TUNING_SINGLE_BACKGROUND_MERGE_LOCAL_TEMP_FILE_PATH { get; set; }
        string AUDIO_TUNING_MULTI_BACKGROUND_MERGE_LOCAL_TEMP_FILE_PATH { get; set; }
        string AUDIO_TUNING_AI_TUNE_LOCAL_TEMP_FILE_PATH { get; set; }
        string AUDIO_TUNING_EQUALIZER_LOCAL_TEMP_FILE_PATH { get; set; }
    }
}
