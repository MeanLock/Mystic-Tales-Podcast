namespace UserService.BusinessLogic.DTOs.Cache
{
    public class AccountStatusCache
    {
        public required int Id { get; set; }
        public int RoleId { get; set; }
        public bool IsVerified { get; set; }
        public int ViolationLevel { get; set; }
        public int ViolationPoint { get; set; }
        public DateTime? LastViolationPointChanged { get; set; }
        public DateTime? LastViolationLevelChanged { get; set; }
        public DateTime? DeactivatedAt { get; set; }
    }

}