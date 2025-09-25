using System.Collections.Generic;

namespace SurveyTalkService.BusinessLogic.DTOs.Survey.Session.V2
{
    public class SurveyEditingSessionDTO
    {
        public int Id { get; set; }
        public int RequesterId { get; set; }
        public string? Title { get; set; } // [field cập nhật]
        public string? Description { get; set; } // [field cập nhật]
        public int SurveyTypeId { get; set; }  // [kiểm tra bước 1]
        public int? SurveyTopicId { get; set; } // [field cập nhật]
        public int? SurveySpecificTopicId { get; set; } // [field cập nhật]
        public string? MainImageBase64 { get; set; } // [field cập nhật]
        public string? BackgroundImageBase64 { get; set; } // [field cập nhật]
        public int? SurveyStatusId { get; set; }
        public int? Version { get; set; }  // [kiểm tra bước 1]
        public int? MarketSurveyVersionStatusId { get; set; }
        public int? SecurityModeId { get; set; } // [field cập nhật]
        public SurveyEditingSessionSurveyConfigJsonDTO? ConfigJson { get; set; } // [field cập nhật]
        public List<SurveyEditingSessionQuestionDTO>? Questions { get; set; } // [kiểm tra bước 1] [field cập nhật] kiểm tra thêm tồn tại null trong surveyTypeId = 2 và SurveyStatusId = 2
    }

    public class SurveyEditingSessionSurveyConfigJsonDTO
    {
        public int? DefaultBackgroundImageId { get; set; } 
        public string? BackgroundGradient1Color { get; set; } 
        public string? BackgroundGradient2Color { get; set; } 
        public string? TitleColor { get; set; } 
        public string? ContentColor { get; set; } 
        public string? ButtonBackgroundColor { get; set; }
        public string? ButtonContentColor { get; set; }
        public bool? IsPause { get; set; }
        public string? Password { get; set; } 
        public int? Brightness { get; set; } 
        public bool? SkipStartPage { get; set; } 
        public string? Background { get; set; } 
        public bool IsUseBackgroundImageBase64 { get; set; } 
    }

    public class SurveyEditingSessionQuestionDTO
    {
        public Guid Id { get; set; } 
        public int? QuestionTypeId { get; set; } // [field cập nhật]
        public string? MainImageBase64 { get; set; } // [field cập nhật]
        public int? Version { get; set; }
        public bool? IsReanswerRequired { get; set; } // [field cập nhật]
        // public int? ReferenceSurveyQuestionId { get; set; } // [field cập nhật]
        // [INT --> GUID]
        public Guid? ReferenceSurveyQuestionId { get; set; } // [field cập nhật]
        public string? Content { get; set; } // [field cập nhật]
        public string? Description { get; set; } // [field cập nhật]
        public int? TimeLimit { get; set; } // [field cập nhật]
        public bool IsVoiced { get; set; } // [field cập nhật]
        public int Order { get; set; } // [field cập nhật]
        public SurveyEditingSessionQuestionConfigJsonDTO? ConfigJson { get; set; } // [field cập nhật] cập nhật string, [CHỈNH LẠI] sau này sẽ kiểm tra questionId/optionId thay vì questionOrder/optionOrder trong DisplayLogic và JumpLogic
        public List<SurveyEditingSessionOptionDTO>? Options { get; set; } // [field cập nhật]
    }

    public class SurveyEditingSessionQuestionConfigJsonDTO
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
        public List<SurveyEditingSessionDisplayLogicDTO>? DisplayLogics { get; set; }
        public List<SurveyEditingSessionJumpLogicDTO>? JumpLogics { get; set; }
    }

    public class SurveyEditingSessionDisplayLogicDTO
    {
        public List<SurveyEditingSessionDisplayLogicConditionDTO>? Conditions { get; set; }
        // public int TargetQuestionOrder { get; set; }
        // [INT --> GUID]
        public Guid? TargetQuestionId { get; set; }
    }

    public class SurveyEditingSessionDisplayLogicConditionDTO
    {
        // public int QuestionOrder { get; set; }
        // [INT --> GUID]
        public Guid QuestionId { get; set; }
        public string? Conjunction { get; set; }
        public string? Operator { get; set; }
        // public int? OptionOrder { get; set; }
        // [INT --> GUID]
        public Guid? OptionId { get; set; }
        public int? CompareValue { get; set; }
    }

    public class SurveyEditingSessionJumpLogicDTO
    {
        public List<SurveyEditingSessionJumpLogicConditionDTO>? Conditions { get; set; }
        // public int TargetQuestionOrder { get; set; }
        // [INT --> GUID]
        public Guid? TargetQuestionId { get; set; }
    }

    public class SurveyEditingSessionJumpLogicConditionDTO
    {
        // public int QuestionOrder { get; set; }
        // [INT --> GUID]
        public Guid QuestionId { get; set; }
        public string? Conjunction { get; set; }
        public string? Operator { get; set; }
        // public int? OptionOrder { get; set; }
        // [INT --> GUID]
        public Guid? OptionId { get; set; }
        public int? CompareValue { get; set; }
    }

    public class SurveyEditingSessionOptionDTO
    {
        public Guid Id { get; set; }
        public string? Content { get; set; } // [field cập nhật]
        public int Order { get; set; } // [field cập nhật]
        public string? MainImageBase64 { get; set; } // [field cập nhật]
    }
}
