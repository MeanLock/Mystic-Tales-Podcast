namespace SurveyTalkService.BusinessLogic.DTOs.Account
{
    public class AccountDetailDTO : AccountListItemDTO
    {
        public AccountProfileDTO Profile { get; set; } = new AccountProfileDTO();
    }


}