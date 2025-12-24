using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModerationService.BusinessLogic.DTOs.DMCANotice
{
    public class DMCANoticeCreateRequestDTO
    {
        public List<IFormFile> DMCANoticeAttachFiles { get; set; } = new List<IFormFile>();
        public string DMCANoticeCreateInfo { get; set; }
    }
    public class DMCANoticeCreateInfoDTO 
    {
        public string AccuserEmail { get; set; }
        public string AccuserPhone { get; set; }    
        public string AccuserFullName { get; set; }
    }
}
