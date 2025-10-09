using Microsoft.Extensions.Configuration;
using UserService.Infrastructure.Configurations.Google.interfaces;

namespace UserService.Infrastructure.Configurations.Google
{
    public class GoogleOAuth2ConfigModel
    {
        public string ClientId_MTP { get; set; } = string.Empty;
        public string ClientSecret_MTP { get; set; } = string.Empty;
    }
    public class GoogleOAuth2Config : IGoogleOAuth2Config
    {
        public string ClientId { get; set; } = string.Empty;
        public string ClientSecret { get; set; } = string.Empty;
        public GoogleOAuth2Config(IConfiguration configuration)
        {
            var googleOAuth2Config = configuration.GetSection("Infrastructure:Google:OAuth2").Get<GoogleOAuth2ConfigModel>();
            if (googleOAuth2Config != null)
            {
                // ClientId = googleOAuth2Config.ClientId_generalTest;
                // ClientSecret = googleOAuth2Config.ClientSecret_generalTest;
                ClientId = googleOAuth2Config.ClientId_MTP;
                ClientSecret = googleOAuth2Config.ClientSecret_MTP;
            }
        }

        
    }
}
