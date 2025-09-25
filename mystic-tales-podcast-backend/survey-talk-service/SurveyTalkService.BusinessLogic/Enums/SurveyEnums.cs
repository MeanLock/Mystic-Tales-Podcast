namespace SurveyTalkService.BusinessLogic.Enums
{
    public enum SurveyTakingSubjectEnum
    {
        Verified = 1,
        Preview = 2,
        Guest = 3,
        LevelUpdate = 3,
    }

    public enum SurveyTakenSubjectEnum
    {
        Verified = 1,
        Guest = 2,
        LevelUpdate = 3,
    }

    public enum SurveyDeadlineQueryEnum
    {
        OnDeadline = 1,
        NearDeadline = 2,
        LateForDeadline = 3
    }

    public enum SurveyAdditionalQueryEnum
    {
        SuitYouBest = 1,
        BigBonus = 2,
        SuitYourFavorite = 3
    }

    public enum SurveyStatusQueryEnum
    {
        Editing = 1,
        Published = 2,
        Completed = 3,
        Deactivated = 4,
    }
}