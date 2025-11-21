using Newtonsoft.Json.Linq;

namespace UserService.Common.AppConfigurations.BusinessSetting.interfaces
{
    public interface ICustomerListenSessionProcedureConfig
    {
        string DefaultPlayOrderMode { get; set; }
        bool DefaultIsAutoPlay { get; set; }
    }
}
