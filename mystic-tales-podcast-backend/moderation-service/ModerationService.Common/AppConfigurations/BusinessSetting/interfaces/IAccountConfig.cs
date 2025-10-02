using Newtonsoft.Json.Linq;

namespace ModerationService.Common.AppConfigurations.BusinessSetting.interfaces
{
    public interface IAccountConfig
    {
        int VerifyCodeLength { get; set; }
    }
}
