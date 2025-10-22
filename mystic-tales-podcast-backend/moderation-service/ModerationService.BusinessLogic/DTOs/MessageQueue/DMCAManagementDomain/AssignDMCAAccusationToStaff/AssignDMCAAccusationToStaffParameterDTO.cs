using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModerationService.BusinessLogic.DTOs.MessageQueue.DMCAManagementDomain.AssignDMCAAccusationToStaff
{
    public class AssignDMCAAccusationToStaffParameterDTO
    {
        public int DMCAAccusationId { get; set; }
        public int AccountId { get; set; }
    }
}
