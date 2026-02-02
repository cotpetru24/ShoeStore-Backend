namespace ShoeStore.Dto.Admin
{
    public class AdminUserDto
    {
        public string Id { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public bool EmailConfirmed { get; set; }
        public bool LockoutEnabled { get; set; }
        public bool IsBlocked { get; set; }
        public DateTime? LockoutEnd { get; set; }
        public int AccessFailedCount { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public int TotalOrders { get; set; }
        public decimal TotalSpent { get; set; }
        public List<string> Roles { get; set; } = new List<string>();
    }
}
