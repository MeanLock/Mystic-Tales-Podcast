using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingManagementService.Common.AppConfigurations.BusinessSetting.interfaces
{
    public interface IDMCAAccusationConfig
    {
        decimal DismissStaticViolationPoint { get; set; }
    }
}
