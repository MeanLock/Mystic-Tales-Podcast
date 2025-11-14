using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModerationService.BusinessLogic.DTOs.MessageQueue.DMCAManagementDomain.CreateLawsuitProof
{
    public class CreateLawsuitProofParameterDTO
    {
        public int DMCAAccusationId { get; set; }
        public List<string> LawsuitProofAttachFileKeys { get; set; } = new List<string>();
    }
}
