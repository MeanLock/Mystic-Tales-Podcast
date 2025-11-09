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
    }
}
