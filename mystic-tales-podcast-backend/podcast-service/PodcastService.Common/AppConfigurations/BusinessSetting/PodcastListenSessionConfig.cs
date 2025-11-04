using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using PodcastService.Common.AppConfigurations.BusinessSetting.interfaces;

namespace PodcastService.Common.AppConfigurations.BusinessSetting
{
    public class PodcastListenSessionConfigModel
    {
        public string TokenSecretKey { get; set; }
        public int TokenListenRequestExpirationMinutes { get; set; }
        public int TokenEncryptionKeyRequestExpirationMinutes { get; set; }
        public int SessionExpirationMinutes { get; set; }
        public int SessionAdditionalUpdateBufferExpirationMinutes { get; set; }
    }
    public class PodcastListenSessionConfig : IPodcastListenSessionConfig
    {
        public string TokenSecretKey { get; set; }
        public int TokenListenRequestExpirationMinutes { get; set; }
        public int TokenEncryptionKeyRequestExpirationMinutes { get; set; }
        public int SessionExpirationMinutes { get; set; }
        public int SessionAdditionalUpdateBufferExpirationMinutes { get; set; }

        public PodcastListenSessionConfig(IConfiguration configuration)
        {
            var podcastListenSessionConfig = configuration.GetSection("BusinessSettings:PodcastListenSession").Get<PodcastListenSessionConfigModel>();
            TokenSecretKey = podcastListenSessionConfig.TokenSecretKey;
            TokenListenRequestExpirationMinutes = podcastListenSessionConfig.TokenListenRequestExpirationMinutes;
            TokenEncryptionKeyRequestExpirationMinutes = podcastListenSessionConfig.TokenEncryptionKeyRequestExpirationMinutes;
            SessionExpirationMinutes = podcastListenSessionConfig.SessionExpirationMinutes;
            SessionAdditionalUpdateBufferExpirationMinutes = podcastListenSessionConfig.SessionAdditionalUpdateBufferExpirationMinutes;
        }
    }
}
