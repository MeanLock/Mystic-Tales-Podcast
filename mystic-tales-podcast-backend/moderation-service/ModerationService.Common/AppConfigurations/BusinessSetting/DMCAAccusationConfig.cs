using Microsoft.Extensions.Configuration;
using ModerationService.Common.AppConfigurations.BusinessSetting.interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModerationService.Common.AppConfigurations.BusinessSetting
{
    public class DMCAAccusationConfigModel
    {
        public int DMCANoticeResponseTime { get; set; }
        public int DMCACounterNoticeResponseTime { get; set; }
        public decimal DismissStaticViolationPoint { get; set; }
        public decimal InvalidDMCACounterNoticePenaltyPoint { get; set; }
        public decimal TakenDownPenaltyPoint { get; set; }
        public decimal LawsuitLosePenaltyPoint { get; set; }
    }
    public class DMCAAccusationConfig : IDMCAAccusationConfig
    {
        public int DMCANoticeResponseTime { get; set; }
        public int DMCACounterNoticeResponseTime { get; set; }
        public decimal DismissStaticViolationPoint { get; set; }
        public decimal InvalidDMCACounterNoticePenaltyPoint { get; set; }
        public decimal TakenDownPenaltyPoint { get; set; }
        public decimal LawsuitLosePenaltyPoint { get; set; }
        public DMCAAccusationConfig(IConfiguration configuration)
        {
            var dmcaAccusationConfig = configuration.GetSection("BusinessSettings:DMCAAccusation").Get<DMCAAccusationConfigModel>();
            DMCANoticeResponseTime = dmcaAccusationConfig?.DMCANoticeResponseTime ?? 14;
            DMCACounterNoticeResponseTime = dmcaAccusationConfig?.DMCACounterNoticeResponseTime ?? 14;
            DismissStaticViolationPoint = dmcaAccusationConfig?.DismissStaticViolationPoint ?? 200;
            InvalidDMCACounterNoticePenaltyPoint = dmcaAccusationConfig?.InvalidDMCACounterNoticePenaltyPoint ?? 10;
            TakenDownPenaltyPoint = dmcaAccusationConfig?.TakenDownPenaltyPoint ?? 200;
            LawsuitLosePenaltyPoint = dmcaAccusationConfig?.LawsuitLosePenaltyPoint ?? 500;
        }
    }
}
