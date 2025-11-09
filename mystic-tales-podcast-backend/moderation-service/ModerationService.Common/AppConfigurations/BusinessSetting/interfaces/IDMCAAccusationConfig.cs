using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModerationService.Common.AppConfigurations.BusinessSetting.interfaces
{
    public interface IDMCAAccusationConfig
    {
        int DMCANoticeResponseTime { get; set; }
        int DMCACounterNoticeResponseTime { get; set; }
        decimal DismissStaticViolationPoint { get; set; }
    }
}
