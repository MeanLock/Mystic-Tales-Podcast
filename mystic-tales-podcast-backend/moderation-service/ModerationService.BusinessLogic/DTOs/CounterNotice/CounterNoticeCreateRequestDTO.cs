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
    }
}
