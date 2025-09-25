using System.Collections.Generic;

namespace SurveyTalkService.BusinessLogic.DTOs.Survey.TakenResult
{
    public class CommunityResponseSummaryListItemDTO
    {
        public List<CommunityResponseSummarySurveyQuestionDTO> Questions { get; set; } = new List<CommunityResponseSummarySurveyQuestionDTO>();
        public List<CommunityResponseSummarySurveyResponseDTO> Responses { get; set; } = new List<CommunityResponseSummarySurveyResponseDTO>();

    }

    public class CommunityResponseSummarySurveyQuestionDTO
    {
        // public int Id { get; set; }
        // [INT --> GUID]
        public Guid Id { get; set; }
        public int SurveyId { get; set; }
        public int QuestionTypeId { get; set; }
        public string Content { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int TimeLimit { get; set; }
        public bool IsVoiced { get; set; }
        public int Order { get; set; }
        public string ConfigJsonString { get; set; } = string.Empty;
        public DateTime? DeletedAt { get; set; }
        public string MainImageUrl { get; set; } = string.Empty;
        public List<CommunityResponseSummarySurveyOptionDTO> Options { get; set; } = new List<CommunityResponseSummarySurveyOptionDTO>();
    }

    public class CommunityResponseSummarySurveyOptionDTO
    {
        // public int Id { get; set; }
        // [INT --> GUID]
        public Guid Id { get; set; }
        // public int SurveyQuestionId { get; set; }
        // [INT --> GUID]
        public Guid SurveyQuestionId { get; set; }
        public string Content { get; set; } = string.Empty;
        public int Order { get; set; }
        public string MainImageUrl { get; set; } = string.Empty;
    }

    public class CommunityResponseSummarySurveyResponseDTO : SurveyResponseDTO
    {
        
    }

}
