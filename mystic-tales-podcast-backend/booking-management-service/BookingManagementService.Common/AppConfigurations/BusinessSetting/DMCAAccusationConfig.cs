using BookingManagementService.Common.AppConfigurations.BusinessSetting.interfaces;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingManagementService.Common.AppConfigurations.BusinessSetting
{
    public class DMCAAccusationConfigModel
    {
        public decimal DismissStaticViolationPoint { get; set; }
    }
    public class DMCAAccusationConfig : IDMCAAccusationConfig
    {
        public decimal DismissStaticViolationPoint { get; set; }
        public DMCAAccusationConfig(IConfiguration configuration)
        {
            var dmcaAccusationConfig = configuration.GetSection("BusinessSettings:DMCAAccusation").Get<DMCAAccusationConfigModel>();
            DismissStaticViolationPoint = dmcaAccusationConfig?.DismissStaticViolationPoint ?? 49;
        }
    }
}
