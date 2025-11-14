namespace UserService.BusinessLogic.Models.Mail
{
    public class CustomerGoogleRegistrationNewAccountPasswordMailViewModel
    {
        public required string Email { get; set; }
        public required string FullName { get; set; }
        public required string NewAccountPassword { get; set; }

    }

}