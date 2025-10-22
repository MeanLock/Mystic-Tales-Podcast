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
        public DMCANoticeCreateInfoDTO DMCANoticeCreateInfo { get; set; }
    }
    public class DMCANoticeCreateInfoDTO
    {
        public int AccountId { get; set; }
        public string AccountEmail { get; set; }
        public string AccountPhone { get; set; }
        public string GoodFaithStatement { get; set; }
        public string WorkClaimed { get; set; }
        public string Signature { get; set; }
    }
}
