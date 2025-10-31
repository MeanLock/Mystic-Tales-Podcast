using Microsoft.Extensions.Configuration;
using PodcastService.Common.AppConfigurations.FilePath.interfaces;

namespace PodcastService.Common.AppConfigurations.FilePath
{
    public class FilePathConfigModel
    {
        public string ACCOUNT_FILE_PATH { get; set; }
        public string ACCOUNT_TEMP_FILE_PATH { get; set; }
        public string PODCAST_CHANNEL_FILE_PATH { get; set; }
        public string PODCAST_CHANNEL_TEMP_FILE_PATH { get; set; }
        public string PODCAST_SHOW_FILE_PATH { get; set; }
        public string PODCAST_SHOW_TEMP_FILE_PATH { get; set; }
        public string PODCAST_EPISODE_FILE_PATH { get; set; }
        public string PODCAST_EPISODE_TEMP_FILE_PATH { get; set; }
        public string PODCAST_BACKGROUND_SOUND_TRACK_FILE_PATH { get; set; }
        public string PODCAST_BACKGROUND_SOUND_TRACK_TEMP_FILE_PATH { get; set; }
        public string BOOKING_FILE_PATH { get; set; }
        public string BOOKING_TEMP_FILE_PATH { get; set; }
        public string DMCA_ACCUSATION_FILE_PATH { get; set; }
        public string DMCA_ACCUSATION_TEMP_FILE_PATH { get; set; }
        
    }
    public class FilePathConfig : IFilePathConfig
    {
        public string ACCOUNT_FILE_PATH { get; set; }
        public string ACCOUNT_TEMP_FILE_PATH { get; set; }
        public string PODCAST_CHANNEL_FILE_PATH { get; set; }
        public string PODCAST_CHANNEL_TEMP_FILE_PATH { get; set; }
        public string PODCAST_SHOW_FILE_PATH { get; set; }
        public string PODCAST_SHOW_TEMP_FILE_PATH { get; set; }
        public string PODCAST_EPISODE_FILE_PATH { get; set; }
        public string PODCAST_EPISODE_TEMP_FILE_PATH { get; set; }
        public string PODCAST_BACKGROUND_SOUND_TRACK_FILE_PATH { get; set; }
        public string PODCAST_BACKGROUND_SOUND_TRACK_TEMP_FILE_PATH { get; set; }
        public string BOOKING_FILE_PATH { get; set; }
        public string BOOKING_TEMP_FILE_PATH { get; set; }
        public string DMCA_ACCUSATION_FILE_PATH { get; set; }
        public string DMCA_ACCUSATION_TEMP_FILE_PATH { get; set; }
        
        public FilePathConfig(IConfiguration configuration)
        {

            var filePaths = configuration.GetSection("FilePaths").Get<FilePathConfigModel>();
            ACCOUNT_FILE_PATH = filePaths.ACCOUNT_FILE_PATH;
            ACCOUNT_TEMP_FILE_PATH = filePaths.ACCOUNT_TEMP_FILE_PATH;
            PODCAST_CHANNEL_FILE_PATH = filePaths.PODCAST_CHANNEL_FILE_PATH;
            PODCAST_CHANNEL_TEMP_FILE_PATH = filePaths.PODCAST_CHANNEL_TEMP_FILE_PATH;
            PODCAST_SHOW_FILE_PATH = filePaths.PODCAST_SHOW_FILE_PATH;
            PODCAST_SHOW_TEMP_FILE_PATH = filePaths.PODCAST_SHOW_TEMP_FILE_PATH;
            PODCAST_EPISODE_FILE_PATH = filePaths.PODCAST_EPISODE_FILE_PATH;
            PODCAST_EPISODE_TEMP_FILE_PATH = filePaths.PODCAST_EPISODE_TEMP_FILE_PATH;
            PODCAST_BACKGROUND_SOUND_TRACK_FILE_PATH = filePaths.PODCAST_BACKGROUND_SOUND_TRACK_FILE_PATH;
            PODCAST_BACKGROUND_SOUND_TRACK_TEMP_FILE_PATH = filePaths.PODCAST_BACKGROUND_SOUND_TRACK_TEMP_FILE_PATH;
            BOOKING_FILE_PATH = filePaths.BOOKING_FILE_PATH;
            BOOKING_TEMP_FILE_PATH = filePaths.BOOKING_TEMP_FILE_PATH;
            DMCA_ACCUSATION_FILE_PATH = filePaths.DMCA_ACCUSATION_FILE_PATH;
            DMCA_ACCUSATION_TEMP_FILE_PATH = filePaths.DMCA_ACCUSATION_TEMP_FILE_PATH;
        }

    }
}
