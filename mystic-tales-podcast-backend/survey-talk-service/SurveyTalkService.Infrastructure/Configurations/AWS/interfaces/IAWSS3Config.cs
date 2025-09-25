namespace SurveyTalkService.Infrastructure.Configurations.AWS.interfaces
{
    public interface IAWSS3Config
    {
        string Profile { get; set; }
        string BucketName { get; set; }
        string Region { get; set; }
        string AccessKey { get; set; }
        string SecretKey { get; set; }
        int PresignedURLExpirationHours { get; set; }
        int UploadImageMaxWidth { get; set; }
        int UploadImageMaxHeight { get; set; }
    }
}
