using ModerationService.BusinessLogic.DTOs.LawsuitProof.ListItems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModerationService.BusinessLogic.DTOs.LawsuitProof.Details
{
    public class LawsuitProofDetailResponseDTO
    {
        public Guid Id { get; set; }
        public int AccountId { get; set; }
        public string GoodFaithStatement { get; set; }
        public string CourtName { get; set; }
        public string CaseNumber { get; set; }
        public DateOnly FilingDate { get; set; }
        public string Signature { get; set; }
        public int DMCAAccusationId { get; set; }
        public bool? IsValid { get; set; }
        public string? InValidReason { get; set; }
        public int? ValidatedBy { get; set; }
        public DateTime? ValidatedAt { get; set; }
        public string? JudgmentDetails { get; set; }
        public DateOnly? DateResolved { get; set; }
        public string? Outcome { get; set; }
        public string? RulingDocumentFileUrl { get; set; }
        public bool? IsDefendantWon { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public List<LawsuitProofAttachFileListItemResponseDTO> LawsuitProofAttachFileList { get; set; }
    }
}
