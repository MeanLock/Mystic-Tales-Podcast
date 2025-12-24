namespace BookingManagementService.BusinessLogic.Models.Mail
{
    public class PodcasterRequestResultMailViewModel
    {
        public required string FullName { get; set; }
        public required string Email { get; set; }
        public required bool IsVerified { get; set; }

    }

}