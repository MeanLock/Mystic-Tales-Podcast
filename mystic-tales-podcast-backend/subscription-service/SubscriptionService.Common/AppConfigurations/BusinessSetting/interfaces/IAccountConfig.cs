using Newtonsoft.Json.Linq;

namespace SubscriptionService.Common.AppConfigurations.BusinessSetting.interfaces
{
    public interface IAccountConfig
    {
        int VerifyCodeLength { get; set; }
        int AccountStatusCacheExpirySeconds { get; set; }
    }
}
