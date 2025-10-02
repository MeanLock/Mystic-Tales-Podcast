namespace SurveyTalkService.BusinessLogic.DTOs.Auth
{
    public class StaffRegisterDTO
    {
        public required StaffRegisterInfoDTO RegisterInfo { get; set; }
    }

    public class StaffRegisterInfoDTO
    {
        public required string Email { get; set; }
        public required string Password { get; set; }
        public required int RoleId { get; set; }
        public required string FullName { get; set; }
        public DateTime Dob { get; set; }
        public string Gender { get; set; }
        public string? Address { get; set; }
        public string Phone { get; set; }
        public string? ImageBase64 { get; set; }
    }
}
