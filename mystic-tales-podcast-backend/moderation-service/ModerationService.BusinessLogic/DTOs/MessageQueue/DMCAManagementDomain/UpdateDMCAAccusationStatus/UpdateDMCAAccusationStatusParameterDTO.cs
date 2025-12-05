using ModerationService.BusinessLogic.Enums.DMCA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModerationService.BusinessLogic.DTOs.MessageQueue.DMCAManagementDomain.UpdateDMCAAccusationStatus
{
    public class UpdateDMCAAccusationStatusParameterDTO
    {
        public int AccountId { get; set; }
        public int DMCAAccusationId { get; set; }
        public int DMCAAccusationAction { get; set; }
        public int? DMCAAccusationTakenDownReasonEnum { get; set; }
        public List<string> AttachmentFileKeys { get; set; } = new List<string>();
    }
}
