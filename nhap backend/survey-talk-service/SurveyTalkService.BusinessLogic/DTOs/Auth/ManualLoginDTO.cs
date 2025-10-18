namespace SurveyTalkService.BusinessLogic.DTOs.Auth
{
    public class ManualLoginDTO
    {
        public required LoginInfoDTO LoginInfo { get; set; }
    }

    public class LoginInfoDTO
    {
        public required string Email { get; set; }
        public required string Password { get; set; }
    }
}
