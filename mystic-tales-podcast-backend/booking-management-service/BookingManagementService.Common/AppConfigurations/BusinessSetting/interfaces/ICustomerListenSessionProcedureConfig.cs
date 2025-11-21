using Newtonsoft.Json.Linq;

namespace BookingManagementService.Common.AppConfigurations.BusinessSetting.interfaces
{
    public interface ICustomerListenSessionProcedureConfig
    {
        string DefaultPlayOrderMode { get; set; }
        bool DefaultIsAutoPlay { get; set; }
    }
}
