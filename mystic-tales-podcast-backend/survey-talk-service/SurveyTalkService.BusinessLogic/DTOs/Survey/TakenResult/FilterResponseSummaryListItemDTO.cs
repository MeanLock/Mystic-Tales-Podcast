using System;
using System.Collections.Generic;

namespace SurveyTalkService.BusinessLogic.DTOs.Survey.TakenResult
{
    public class FilterResponseSummaryListItemDTO
    {
        public List<FilterResponseSummarySurveyQuestionDTO> Questions { get; set; } = new List<FilterResponseSummarySurveyQuestionDTO>();
        public List<FilterResponseSummarySurveyResponseDTO> Responses { get; set; } = new List<FilterResponseSummarySurveyResponseDTO>();
    }

    public class FilterResponseSummarySurveyQuestionDTO
    {
        // public int Id { get; set; }
        // [INT --> GUID]
        public Guid Id { get; set; }
        public int SurveyId { get; set; }
        public int QuestionTypeId { get; set; }
        public string Content { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int Order { get; set; }
        public string ConfigJsonString { get; set; } = string.Empty;
        public DateTime? DeletedAt { get; set; }
        public string MainImageUrl { get; set; } = string.Empty;
        public List<FilterResponseSummarySurveyOptionDTO> Options { get; set; } = new List<FilterResponseSummarySurveyOptionDTO>();
    }

    public class FilterResponseSummarySurveyOptionDTO
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

    public class FilterResponseSummarySurveyResponseDTO : SurveyResponseDTO
    {

    }
}