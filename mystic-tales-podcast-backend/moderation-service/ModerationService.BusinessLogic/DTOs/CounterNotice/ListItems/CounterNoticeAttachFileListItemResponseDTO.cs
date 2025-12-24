using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModerationService.BusinessLogic.DTOs.CounterNotice.ListItems
{
    public class CounterNoticeAttachFileListItemResponseDTO
    {
        public Guid Id { get; set; }
        public string AttachFileKey { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
