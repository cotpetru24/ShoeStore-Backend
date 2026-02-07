namespace ShoeStore.Dto.Admin
{
    public class AdminUserDto
    {
        public string Id { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public bool EmailConfirmed { get; set; }
        public bool IsBlocked { get; set; }
        public DateTime? CreatedAt { get; set; }
        public List<UserRoleEnum> Roles { get; set; } = new List<UserRoleEnum>();
    }
}
