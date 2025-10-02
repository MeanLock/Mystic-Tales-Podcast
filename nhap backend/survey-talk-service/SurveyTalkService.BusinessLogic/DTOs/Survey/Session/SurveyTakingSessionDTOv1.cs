using System.Collections.Generic;

namespace SurveyTalkService.BusinessLogic.DTOs.Survey.Session.V2
{
    public class SurveyTakingSessionDTO
    {
        public int Id { get; set; }
        public int RequesterId { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public int SurveyTypeId { get; set; }
        public int? SurveyTopicId { get; set; }
        public int? SurveySpecificTopicId { get; set; }
        public string? MainImageUrl { get; set; }
        public string? BackgroundImageUrl { get; set; }
        public int? SurveyStatusId { get; set; }
        public int? Version { get; set; }
        public int? MarketSurveyVersionStatusId { get; set; }
        public int? SecurityModeId { get; set; }
        public SurveyTakingSessionConfigJsonDTO? ConfigJson { get; set; }
        public List<SurveyTakingSessionQuestionDTO>? Questions { get; set; }
    }

    public class SurveyTakingSessionConfigJsonDTO
    {
        public string? BackgroundGradient1Color { get; set; }
        public string? BackgroundGradient2Color { get; set; }
        public string? TitleColor { get; set; }
        public string? ContentColor { get; set; }
        public string? ButtonBackgroundColor { get; set; }
        public string? ButtonContentColor { get; set; }
        public string? Password { get; set; }
        public int? Brightness { get; set; }
        public bool? IsPause { get; set; }
        public bool? SkipStartPage { get; set; }
        public string? Background { get; set; } // color_gradient, image
    }

    public class SurveyTakingSessionQuestionDTO
    {
        // public int Id { get; set; }
        // [INT --> GUID]
        public Guid Id { get; set; }
        public int QuestionTypeId { get; set; }
        public int? Version { get; set; }
        public string? MainImageUrl { get; set; }
        public string? Content { get; set; }
        public string? Description { get; set; }
        public int? TimeLimit { get; set; }
        public bool? IsVoiced { get; set; }
        public int Order { get; set; }
        public SurveyTakingSessionQuestionConfigJsonDTO? ConfigJson { get; set; }
        public List<SurveyTakingSessionOptionDTO>? Options { get; set; }
    }

    public class SurveyTakingSessionQuestionConfigJsonDTO
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
        public bool? RequiredAnswer { get; set; }
        public bool? ViewNumberQuestion { get; set; }
        public bool? NotBack { get; set; }
        public bool? ImageEndQuestion { get; set; }
        public bool? IsUseLabel { get; set; }
        public List<SurveyTakingSessionDisplayLogicDTO>? DisplayLogics { get; set; }
        public List<SurveyTakingSessionJumpLogicDTO>? JumpLogics { get; set; }
    }

    public class SurveyTakingSessionDisplayLogicDTO
    {
        public List<SurveyTakingSessionLogicConditionDTO>? Conditions { get; set; }
        // public int TargetQuestionId { get; set; }
        // [INT --> GUID]
        public Guid? TargetQuestionId { get; set; }
    }

    public class SurveyTakingSessionJumpLogicDTO
    {
        public List<SurveyTakingSessionLogicConditionDTO>? Conditions { get; set; }
        // public int TargetQuestionId { get; set; }
        // [INT --> GUID]
        public Guid? TargetQuestionId { get; set; }
    }

    public class SurveyTakingSessionLogicConditionDTO
    {
        // public int QuestionId { get; set; }
        // [INT --> GUID]
        public Guid QuestionId { get; set; }
        public string? Conjunction { get; set; }
        public string? Operator { get; set; }
        // public int? OptionId { get; set; }
        // [INT --> GUID]
        public Guid? OptionId { get; set; }
        public int? CompareValue { get; set; }
    }

    public class SurveyTakingSessionOptionDTO
    {
        // public int Id { get; set; }
        // [INT --> GUID]
        public Guid? Id { get; set; }
        public string? Content { get; set; }
        public int Order { get; set; }
        public string? MainImageUrl { get; set; }
    }
}
