using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModerationService.BusinessLogic.DTOs.DMCAReport
{
    public class DMCAAccusationConclusionReportCreateRequestDTO
    {
        public int DmcaAccusationConclusionReportTypeId { get; set; }
        public string? Description { get; set; }
        public string? InvalidReason { get; set; }
    }
}
