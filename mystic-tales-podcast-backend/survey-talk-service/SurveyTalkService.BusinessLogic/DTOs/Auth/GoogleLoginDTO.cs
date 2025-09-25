namespace SurveyTalkService.BusinessLogic.DTOs.Auth
{
    public class GoogleLoginDTO
    {
        public required GoogleAuthDTO GoogleAuth { get; set; }
    }

    public class GoogleAuthDTO
    {
        public required string AuthorizationCode { get; set; }
        public required string RedirectUri { get; set; }
    }
}
