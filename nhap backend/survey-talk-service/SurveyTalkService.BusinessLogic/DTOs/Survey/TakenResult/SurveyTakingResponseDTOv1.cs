using System.Collections.Generic;

namespace SurveyTalkService.BusinessLogic.DTOs.Survey.TakenResult.V1
{
    public class SurveyTakingResponseDTO
    {
        public bool IsValid { get; set; }
        public SurveyTakingResponseValueJsonDTO? ValueJson { get; set; }
    }

    public class SurveyTakingResponseValueJsonDTO
    {
        public SurveyResponseQuestionContentDTO? QuestionContent { get; set; }
        public SurveyResponseQuestionResponseDTO? QuestionResponse { get; set; }
    }

    public class SurveyResponseQuestionContentDTO
    {
        // public int Id { get; set; }
        // [INT --> GUID]
        public Guid Id { get; set; }
        public string? MainImageUrl { get; set; }
        public int QuestionTypeId { get; set; }
        public string? Content { get; set; }
        public string? Description { get; set; }
        public SurveyResponseQuestionConfigJsonDTO? ConfigJson { get; set; }
        public List<SurveyResponseOptionDTO>? Options { get; set; }
    }

    public class SurveyResponseQuestionConfigJsonDTO
    {
        public int? Min { get; set; }
        public int? Max { get; set; }
        public int? Step { get; set; }
        public string? Unit { get; set; }
        public int? RatingLength { get; set; }
        public string? RatingIcon { get; set; }
        public int? MinChoiceCount { get; set; }
        public int? MaxChoiceCount { get; set; }
        public int? FieldInputTypeId { get; set; }
    }

    public class SurveyResponseOptionDTO
    {
        // public int Id { get; set; }
        // [INT --> GUID]
        public Guid Id { get; set; }
        public string? Content { get; set; }
        public int Order { get; set; }
        public string? MainImageUrl { get; set; }
    }

    public class SurveyResponseQuestionResponseDTO
    {
        public SurveyResponseInputDTO? Input { get; set; }
        public SurveyResponseRangeDTO? Range { get; set; }
        public List<SurveyResponseRankingDTO>? Ranking { get; set; }
        // public int? SingleChoice { get; set; }
        // public List<int>? MultipleChoice { get; set; }
        // [INT --> GUID]
        public Guid? SingleChoice { get; set; }
        public List<Guid>? MultipleChoice { get; set; }
        public string? SpeechText { get; set; }
    }

    public class SurveyResponseInputDTO
    {
        public object? Value { get; set; } // string hoặc number
        public string? ValueType { get; set; }
    }

    public class SurveyResponseRangeDTO
    {
        public double? Min { get; set; }
        public double? Max { get; set; }
    }

    public class SurveyResponseRankingDTO
    {
        // public int SurveyOptionId { get; set; }
        // [INT --> GUID]
        public Guid SurveyOptionId { get; set; }
        public int RankIndex { get; set; }
    }
}
