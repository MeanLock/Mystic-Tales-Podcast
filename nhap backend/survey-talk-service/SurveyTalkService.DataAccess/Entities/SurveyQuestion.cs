using System;
using System.Collections.Generic;

namespace SurveyTalkService.DataAccess.Entities;

public partial class SurveyQuestion
{
    public int SurveyId { get; set; }

    public int? QuestionTypeId { get; set; }

    public string Content { get; set; } = null!;

    public string? Description { get; set; }

    public int TimeLimit { get; set; }

    public bool IsVoiced { get; set; }

    public byte Order { get; set; }

    public string ConfigJsonString { get; set; } = null!;

    public DateTime? DeletedAt { get; set; }

    public byte? Version { get; set; }

    public bool? IsReanswerRequired { get; set; }

    public Guid Id { get; set; }

    public Guid? ReferenceSurveyQuestionId { get; set; }

    public virtual ICollection<SurveyQuestion> InverseReferenceSurveyQuestion { get; set; } = new List<SurveyQuestion>();

    public virtual SurveyQuestionType? QuestionType { get; set; }

    public virtual SurveyQuestion? ReferenceSurveyQuestion { get; set; }

    public virtual Survey Survey { get; set; } = null!;

    public virtual ICollection<SurveyOption> SurveyOptions { get; set; } = new List<SurveyOption>();

    public virtual ICollection<SurveyResponse> SurveyResponses { get; set; } = new List<SurveyResponse>();
}
