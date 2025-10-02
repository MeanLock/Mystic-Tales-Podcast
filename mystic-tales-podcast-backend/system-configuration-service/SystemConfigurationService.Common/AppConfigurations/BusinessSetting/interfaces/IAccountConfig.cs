using Newtonsoft.Json.Linq;

namespace SystemConfigurationService.Common.AppConfigurations.BusinessSetting.interfaces
{
    public interface IAccountConfig
    {
        int VerifyCodeLength { get; set; }
    }
}
