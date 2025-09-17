using Microsoft.Extensions.Configuration;
using Architecture_1.Infrastructure.Configurations.OpenAI.interfaces;

namespace Architecture_1.Infrastructure.Configurations.OpenAI
{
    public class OpenAIConfigModel
    {
        public string BaseUrl { get; set; }
        public string BaseModel { get; set; }
        public string ApiKey { get; set; }
    }
    public class OpenAIConfig : IOpenAIConfig
    {
        public string ApiKey { get; set; }
        public string BaseModel { get; set; }
        public string BaseUrl { get; set; }
        public OpenAIConfig(IConfiguration configuration)
        {
            var openaiConfig = configuration.GetSection("Infrastructure:OpenAI").Get<OpenAIConfigModel>();
            ApiKey = openaiConfig.ApiKey;
            BaseUrl = openaiConfig.BaseUrl;
            BaseModel = openaiConfig.BaseModel;
        }

        
    }
}
