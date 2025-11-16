using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModerationService.BusinessLogic.DTOs.DMCAReport.ListItems
{
    public class DMCAAccusationConclusionReportListItemResponseDTO
    {
        public Guid Id { get; set; }
        public int DmcaAccusationId { get; set; }
        public DMCAAccusationConclusionReportTypeResponseDTO DmcaAccusationConclusionReportType { get; set; }
        public string? Description { get; set; }
        public string? InvalidReason { get; set; }
        public bool? IsRejected { get; set; }
        public DateTime? CompletedAt { get; set; }
        public DateTime? CancelledAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
