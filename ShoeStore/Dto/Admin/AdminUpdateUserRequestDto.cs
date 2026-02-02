namespace ShoeStore.Dto.Admin
{
    public class AdminUpdateUserRequestDto
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public bool? LockoutEnabled { get; set; }
        public bool? IsBlocked { get; set; }
        public DateTime? LockoutEnd { get; set; }
        public string? Email { get; set; }
        public List<string>? Roles { get; set; } = new List<string>();
    }
}
