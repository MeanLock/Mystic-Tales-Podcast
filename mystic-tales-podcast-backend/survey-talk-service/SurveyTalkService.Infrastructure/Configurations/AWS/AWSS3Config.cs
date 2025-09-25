using Microsoft.Extensions.Configuration;
using SurveyTalkService.Infrastructure.Configurations.AWS.interfaces;

namespace SurveyTalkService.Infrastructure.Configurations.AWS
{
    public class AWSS3ConfigModel
    {
        public string Profile { get; set; } = string.Empty;
        public string BucketName { get; set; } = string.Empty;
        public string Region { get; set; } = string.Empty;
        public string AccessKey { get; set; } = string.Empty;
        public string SecretKey { get; set; } = string.Empty;
        public int PresignedURLExpirationHours { get; set; }
        public int UploadImageMaxWidth { get; set; }
        public int UploadImageMaxHeight { get; set; }
    }

    public class AWSS3Config : IAWSS3Config
    {
        public string Profile { get; set; } = string.Empty;
        public string BucketName { get; set; } = string.Empty;
        public string Region { get; set; } = string.Empty;
        public string AccessKey { get; set; } = string.Empty;
        public string SecretKey { get; set; } = string.Empty;
        public int PresignedURLExpirationHours { get; set; }
        public int UploadImageMaxWidth { get; set; }
        public int UploadImageMaxHeight { get; set; }


        public AWSS3Config(IConfiguration configuration)
        {
            var awsS3Config = configuration.GetSection("Infrastructure:AWS:S3").Get<AWSS3ConfigModel>();
            if (awsS3Config != null)
            {
                Profile = awsS3Config.Profile;
                BucketName = awsS3Config.BucketName;
                Region = awsS3Config.Region;
                AccessKey = awsS3Config.AccessKey;
                SecretKey = awsS3Config.SecretKey;
                PresignedURLExpirationHours = awsS3Config.PresignedURLExpirationHours;
                UploadImageMaxWidth = awsS3Config.UploadImageMaxWidth;
                UploadImageMaxHeight = awsS3Config.UploadImageMaxHeight;
            }
        }

        
    }
}
