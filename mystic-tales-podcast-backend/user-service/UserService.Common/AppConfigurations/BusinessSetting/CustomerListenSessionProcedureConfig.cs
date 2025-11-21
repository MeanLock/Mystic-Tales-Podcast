using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using UserService.Common.AppConfigurations.BusinessSetting.interfaces;

namespace UserService.Common.AppConfigurations.BusinessSetting
{
    public class CustomerListenSessionProcedureConfigModel
    {
        public string DefaultPlayOrderMode { get; set; }
        public bool DefaultIsAutoPlay { get; set; }
    }
    public class CustomerListenSessionProcedureConfig : ICustomerListenSessionProcedureConfig
    {
        public string DefaultPlayOrderMode { get; set; }
        public bool DefaultIsAutoPlay { get; set; }


        public CustomerListenSessionProcedureConfig(IConfiguration configuration)
        {
            var customerListenSessionProcedureConfig = configuration.GetSection("BusinessSettings:CustomerListenSessionProcedure").Get<CustomerListenSessionProcedureConfigModel>();
            DefaultPlayOrderMode = customerListenSessionProcedureConfig.DefaultPlayOrderMode; 
            DefaultIsAutoPlay = customerListenSessionProcedureConfig.DefaultIsAutoPlay;
        }
    }
}
