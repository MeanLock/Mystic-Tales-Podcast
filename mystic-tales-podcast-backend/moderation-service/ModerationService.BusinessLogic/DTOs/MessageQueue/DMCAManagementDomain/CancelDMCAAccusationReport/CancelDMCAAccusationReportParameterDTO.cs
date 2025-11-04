using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModerationService.BusinessLogic.DTOs.MessageQueue.DMCAManagementDomain.CancelDMCAAccusationReport
{
    public class CancelDMCAAccusationReportParameterDTO
    {
        public int AccountId { get; set; }
        public Guid DMCAAccusationConclusionReportId { get; set; }
    }
}
