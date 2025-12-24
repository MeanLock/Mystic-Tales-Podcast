using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModerationService.BusinessLogic.DTOs.MessageQueue.DMCAManagementDomain.ValidateDMCAAccusationReport
{
    public class ValidateDMCAAccusationReportParameterDTO
    {
        public Guid DMCAAccusationConclusionReportId { get; set; }
        public bool IsValid { get; set; }
    }
}
