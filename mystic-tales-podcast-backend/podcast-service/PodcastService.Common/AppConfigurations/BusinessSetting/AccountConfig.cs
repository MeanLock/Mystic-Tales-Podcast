using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using PodcastService.Common.AppConfigurations.BusinessSetting.interfaces;

namespace PodcastService.Common.AppConfigurations.BusinessSetting
{
    public class AccountConfigModel
    {
        public int VerifyCodeLength { get; set; }
        public int AccountStatusCacheExpirySeconds { get; set; }
        
    }
    public class AccountConfig : IAccountConfig
    {
        public int VerifyCodeLength { get; set; }
        public int AccountStatusCacheExpirySeconds { get; set; }


        public AccountConfig(IConfiguration configuration)
        {
            var accountConfig = configuration.GetSection("BusinessSettings:Account").Get<AccountConfigModel>();
            VerifyCodeLength = accountConfig?.VerifyCodeLength ?? 6; 
            AccountStatusCacheExpirySeconds = accountConfig?.AccountStatusCacheExpirySeconds ?? 3600;
        }
    }
}
