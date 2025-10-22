using Newtonsoft.Json.Linq;

namespace SurveyTalkService.Common.AppConfigurations.BusinessSetting.interfaces
{
    public interface IAccountConfig
    {
        int VerifyCodeLength { get; set; }
    }
}
