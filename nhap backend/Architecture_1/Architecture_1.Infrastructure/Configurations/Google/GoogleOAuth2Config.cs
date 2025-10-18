using Microsoft.Extensions.Configuration;
using Architecture_1.Infrastructure.Configurations.Google.interfaces;

namespace Architecture_1.Infrastructure.Configurations.Google
{
    public class GoogleOAuth2ConfigModel
    {
        public string ClientId_SurveyTalk { get; set; }
        public string ClientSecret_SurveyTalk { get; set; }
    }
    public class GoogleOAuth2Config : IGoogleOAuth2Config
    {
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public GoogleOAuth2Config(IConfiguration configuration)
        {
            var googleOAuth2Config = configuration.GetSection("Infrastructure:Google:OAuth2").Get<GoogleOAuth2ConfigModel>();
            // ClientId = googleOAuth2Config.ClientId_generalTest;
            // ClientSecret = googleOAuth2Config.ClientSecret_generalTest;
            ClientId = googleOAuth2Config.ClientId_SurveyTalk;
            ClientSecret = googleOAuth2Config.ClientSecret_SurveyTalk;
        }

        
    }
}
