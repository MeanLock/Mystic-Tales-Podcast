using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModerationService.BusinessLogic.DTOs.CounterNotice
{
    public class CounterNoticeCreateRequestDTO
    {
        public List<IFormFile> CounterNoticeAttachFiles { get; set; } = new List<IFormFile>();
        public CounterNoticeCreateInfoDTO CounterNoticeCreateInfo { get; set; }
    }
    public class CounterNoticeCreateInfoDTO
    {
        public int AccountId { get; set; }
        public string AccountEmail { get; set; }
        public string AccountPhone { get; set; }
        public string StatementPerjury { get; set; }
        public string Signature { get; set; }
        public string Juridisction { get; set; }
        public DateTime FiledDate { get; set; }
    }
}
