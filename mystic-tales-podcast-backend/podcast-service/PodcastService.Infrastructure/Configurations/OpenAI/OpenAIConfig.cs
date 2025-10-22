using Microsoft.Extensions.Configuration;
using PodcastService.Infrastructure.Configurations.OpenAI.interfaces;

namespace PodcastService.Infrastructure.Configurations.OpenAI
{
    public class OpenAIConfigModel
    {
        public string BaseUrl { get; set; } = string.Empty;
        public string BaseModel { get; set; } = string.Empty;
        public string ApiKey { get; set; } = string.Empty;
    }
    public class OpenAIConfig : IOpenAIConfig
    {
        public string ApiKey { get; set; } = string.Empty;
        public string BaseModel { get; set; } = string.Empty;
        public string BaseUrl { get; set; } = string.Empty;
        public OpenAIConfig(IConfiguration configuration)
        {
            var openaiConfig = configuration.GetSection("Infrastructure:OpenAI").Get<OpenAIConfigModel>();
            if (openaiConfig != null)
            {
                ApiKey = openaiConfig.ApiKey;
                BaseUrl = openaiConfig.BaseUrl;
                BaseModel = openaiConfig.BaseModel;
            }
        }

        
    }
}
