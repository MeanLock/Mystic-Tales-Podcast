using ModerationService.BusinessLogic.DTOs.DMCANotice.ListItems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace ModerationService.BusinessLogic.DTOs.DMCANotice.Details
{
    public class DMCANoticeDetailResponseDTO
    {
        public Guid Id { get; set; }
        public bool? IsValid { get; set; }
        public string? InValidReason { get; set; }
        public string? ValidatedBy { get; set; }
        public DateTime? ValidatedAt { get; set; }
        public int DMCAAccusationId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public List<DMCANoticeAttachFileListItemResponseDTO> DMCANoticeAttachFileList { get; set; }
    }
}
