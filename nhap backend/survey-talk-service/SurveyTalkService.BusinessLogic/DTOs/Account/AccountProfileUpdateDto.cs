namespace SurveyTalkService.BusinessLogic.DTOs.Account
{
    public class AccountProfileUpdateDTO
    {
        public AccountProfileDTO AccountProfile { get; set; } = null!;
        public List<SurveyTopicFavoriteDTO> SurveyTopicFavorites { get; set; } = new();
    }


    
}