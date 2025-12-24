using Newtonsoft.Json.Linq;

namespace BookingManagementService.Common.AppConfigurations.BusinessSetting.interfaces
{
    public interface IAccountConfig
    {
        int VerifyCodeLength { get; set; }
        int AccountStatusCacheExpirySeconds { get; set; }
    }
}
