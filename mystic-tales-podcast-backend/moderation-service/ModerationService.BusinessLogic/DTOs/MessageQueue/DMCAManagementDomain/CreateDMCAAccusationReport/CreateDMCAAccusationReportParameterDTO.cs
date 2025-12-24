using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModerationService.BusinessLogic.DTOs.MessageQueue.DMCAManagementDomain.CreateDMCAAccusationReport
{
    public class CreateDMCAAccusationReportParameterDTO
    {
        public int AccountId { get; set; }
        public int DmcaAccusationId { get; set; }
        public int DmcaAccusationConclusionReportTypeId { get; set; }
        public string? Description { get; set; }
        public string? InvalidReason { get; set; }

    }
}
