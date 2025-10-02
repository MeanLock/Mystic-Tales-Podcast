using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using SystemConfigurationService.Common.AppConfigurations.BusinessSetting.interfaces;

namespace SystemConfigurationService.Common.AppConfigurations.BusinessSetting
{
    public class AccountConfigModel
    {
        public int VerifyCodeLength { get; set; }
        
    }
    public class AccountConfig : IAccountConfig
    {
        public int VerifyCodeLength { get; set; }


        public AccountConfig(IConfiguration configuration)
        {
            var accountConfig = configuration.GetSection("BusinessSettings:Account").Get<AccountConfigModel>();
            VerifyCodeLength = accountConfig?.VerifyCodeLength ?? 6; 
        }
    }
}
