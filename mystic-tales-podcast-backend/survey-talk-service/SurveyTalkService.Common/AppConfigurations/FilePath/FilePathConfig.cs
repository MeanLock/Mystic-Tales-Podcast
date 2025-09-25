using Microsoft.Extensions.Configuration;
using SurveyTalkService.Common.AppConfigurations.FilePath.interfaces;

namespace SurveyTalkService.Common.AppConfigurations.FilePath
{
    public class FilePathConfigModel
    {
        public string ACCOUNt_IMAGE_PATH { get; set; }
        public string SURVEY_ORIGINAL_IMAGE_PATH { get; set; }
        public string SURVEY_DEFAULT_BACKGROUND_IMAGE_PATH { get; set; }
        
    }
    public class FilePathConfig : IFilePathConfig
    {
        public string ACCOUNt_IMAGE_PATH { get; set; }
        public string SURVEY_ORIGINAL_IMAGE_PATH { get; set; }
        public string SURVEY_DEFAULT_BACKGROUND_IMAGE_PATH { get; set; }
        public FilePathConfig(IConfiguration configuration)
        {

            var filePaths = configuration.GetSection("FilePaths").Get<FilePathConfigModel>();
            ACCOUNt_IMAGE_PATH = filePaths.ACCOUNt_IMAGE_PATH;
            SURVEY_ORIGINAL_IMAGE_PATH = filePaths.SURVEY_ORIGINAL_IMAGE_PATH;
            SURVEY_DEFAULT_BACKGROUND_IMAGE_PATH = filePaths.SURVEY_DEFAULT_BACKGROUND_IMAGE_PATH;
        }

    }
}
