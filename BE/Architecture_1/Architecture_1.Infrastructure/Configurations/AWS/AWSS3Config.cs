using Microsoft.Extensions.Configuration;
using Architecture_1.Infrastructure.Configurations.AWS.interfaces;

namespace Architecture_1.Infrastructure.Configurations.AWS
{
    public class AWSS3ConfigModel
    {
        public string Profile { get; set; }
        public string BucketName { get; set; }
        public string Region { get; set; }
        public string AccessKey { get; set; }
        public string SecretKey { get; set; }
        public int PresignedURLExpirationHours { get; set; }
        public int UploadImageMaxWidth { get; set; }
        public int UploadImageMaxHeight { get; set; }
    }

    public class AWSS3Config : IAWSS3Config
    {
        public string Profile { get; set; }
        public string BucketName { get; set; }
        public string Region { get; set; }
        public string AccessKey { get; set; }
        public string SecretKey { get; set; }
        public int PresignedURLExpirationHours { get; set; }
        public int UploadImageMaxWidth { get; set; }
        public int UploadImageMaxHeight { get; set; }


        public AWSS3Config(IConfiguration configuration)
        {
            var awsS3Config = configuration.GetSection("Infrastructure:AWS:S3").Get<AWSS3ConfigModel>();
            Profile = awsS3Config.Profile;
            BucketName = awsS3Config.BucketName;
            Region = awsS3Config.Region;
            AccessKey = awsS3Config.AccessKey;
            SecretKey = awsS3Config.SecretKey;
            PresignedURLExpirationHours = int.Parse(awsS3Config.PresignedURLExpirationHours.ToString());
            UploadImageMaxWidth = int.Parse(awsS3Config.UploadImageMaxWidth.ToString());
            UploadImageMaxHeight = int.Parse(awsS3Config.UploadImageMaxHeight.ToString());


        }

        
    }
}
