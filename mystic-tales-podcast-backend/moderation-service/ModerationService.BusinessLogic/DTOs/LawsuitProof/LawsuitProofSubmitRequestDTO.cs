using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModerationService.BusinessLogic.DTOs.LawsuitProof
{
    public class LawsuitProofSubmitRequestDTO
    {
        public List<IFormFile> LawsuitProofAttachFileKeys { get; set; } = new List<IFormFile>();
        public LawsuitProofCreateInfoDTO LawsuitProofCreateInfo { get; set; }
    }
    public class LawsuitProofCreateInfoDTO
    {
        public int AccountId { get; set; }
        public string GoodFaithStatement { get; set; }
        public string CourtName { get; set; }
        public string CaseNumber { get; set; }
        public DateTime FilingDate { get; set; }
        public string Signature { get; set; }
    }
}
