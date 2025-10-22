using ModerationService.BusinessLogic.DTOs.CounterNotice.ListItems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModerationService.BusinessLogic.DTOs.CounterNotice.Details
{
    public class CounterNoticeDetailResponseDTO
    {
        public Guid Id { get; set; }
        public int AccountId { get; set; }
        public string AccountEmail { get; set; }
        public string AccountPhone { get; set; }
        public string StatementPerjury { get; set; }
        public string Signature { get; set; }
        public int DMCAAccusationId { get; set; }
        public string Jurisdiction { get; set; }
        public bool? IsValid { get; set; }
        public string? InValidReason { get; set; }
        public int? ValidatedBy { get; set; }
        public DateTime? ValidatedAt { get; set; }
        public DateOnly FiledDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public List<CounterNoticeAttachFileListItemResponseDTO> CounterNoticeAttachFileList { get; set; }
    }
}
