namespace UserService.BusinessLogic.DTOs.ViewModels.Mail
{
    public class PodcasterRequestResultMailViewModel
    {
        public required string FullName { get; set; }
        public required string Email { get; set; }
        public required bool IsVerified { get; set; }

    }

}