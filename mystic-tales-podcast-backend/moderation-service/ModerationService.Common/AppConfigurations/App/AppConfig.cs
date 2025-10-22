using Microsoft.Extensions.Configuration;
using ModerationService.Common.AppConfigurations.App.interfaces;

namespace ModerationService.Common.AppConfigurations.App
{
    public class ResetPasswordConfig
    {
        public string? Url { get; set; }
        public int TokenExpiredInMinutes { get; set; }
    }
    public class AppConfigModel
    {
        public int APP_PORT { get; set; }
        public string? APP_BASE_URL { get; set; }
        public string? API_GATEWAY_URL { get; set; }
        public string? HEALTH_CHECK_ENDPOINT { get; set; }
        public ResetPasswordConfig? RESET_PASSWORD { get; set; }
        public string? FILE_STORAGE_SRC { get; set; }
        public string? TIME_ZONE { get; set; }
        public string? EMBEDDING_VECTOR_API_URL { get; set; }
    }
    public class AppConfig : IAppConfig
    {
        public int APP_PORT { get; set; }
        public string? APP_BASE_URL { get; set; }
        public string? API_GATEWAY_URL { get; set; }
        public string? HEALTH_CHECK_ENDPOINT { get; set; }
        public ResetPasswordConfig? RESET_PASSWORD { get; set; }
        public string? FILE_STORAGE_SRC { get; set; }
        public string? TIME_ZONE { get; set; }
        public string? EMBEDDING_VECTOR_API_URL { get; set; }


        public AppConfig(IConfiguration configuration)
        {
            var appConfig = configuration.GetSection("AppSettings").Get<AppConfigModel>();
            APP_PORT = appConfig?.APP_PORT ?? 0;
            APP_BASE_URL = appConfig?.APP_BASE_URL;
            API_GATEWAY_URL = appConfig?.API_GATEWAY_URL;
            HEALTH_CHECK_ENDPOINT = appConfig?.HEALTH_CHECK_ENDPOINT;
            FILE_STORAGE_SRC = appConfig?.FILE_STORAGE_SRC;
            TIME_ZONE = appConfig?.TIME_ZONE;
            RESET_PASSWORD = appConfig?.RESET_PASSWORD;
            EMBEDDING_VECTOR_API_URL = appConfig?.EMBEDDING_VECTOR_API_URL;
           
        }

        
    }
}
