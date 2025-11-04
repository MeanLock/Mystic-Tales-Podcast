using Newtonsoft.Json.Linq;

namespace PodcastService.Common.AppConfigurations.BusinessSetting.interfaces
{
    public interface IPodcastListenSessionConfig
    {
        string TokenSecretKey { get; set; }
        int TokenListenRequestExpirationMinutes { get; set; }
        int TokenEncryptionKeyRequestExpirationMinutes { get; set; }
        int SessionExpirationMinutes { get; set; }
        int SessionAdditionalUpdateBufferExpirationMinutes { get; set; }
    }
}
