namespace PodcastService.Infrastructure.Configurations.OpenAI.interfaces
{
    public interface IOpenAIConfig
    {
        string BaseUrl { get; set; }
        string BaseModel { get; set; }
        string ApiKey { get; set; }
    }
}
