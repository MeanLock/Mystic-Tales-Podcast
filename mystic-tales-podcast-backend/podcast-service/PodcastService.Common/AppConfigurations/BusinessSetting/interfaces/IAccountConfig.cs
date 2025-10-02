using Newtonsoft.Json.Linq;

namespace PodcastService.Common.AppConfigurations.BusinessSetting.interfaces
{
    public interface IAccountConfig
    {
        int VerifyCodeLength { get; set; }
    }
}
