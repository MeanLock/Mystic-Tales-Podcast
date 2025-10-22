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
        public int AccountId { get; set; }
        public string GoodFaithStatement { get; set; }
        public string CourtName { get; set; }
        public string CaseNumber { get; set; }
        public DateTime FilingDate { get; set; }
        public string Signature { get; set; }
        public List<string> LawsuitProofFileKeys { get; set; } = new List<string>();
    }
}
