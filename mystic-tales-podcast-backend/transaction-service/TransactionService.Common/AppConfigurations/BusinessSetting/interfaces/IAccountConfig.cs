using Newtonsoft.Json.Linq;

namespace TransactionService.Common.AppConfigurations.BusinessSetting.interfaces
{
    public interface IAccountConfig
    {
        int VerifyCodeLength { get; set; }
    }
}
