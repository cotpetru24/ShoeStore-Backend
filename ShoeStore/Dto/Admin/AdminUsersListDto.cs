namespace ShoeStore.Dto.Admin
{
    public class AdminUsersListDto
    {
        public List<AdminUserDto> Users { get; set; } = new List<AdminUserDto>();
        public int TotalQueryCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public AdminUsersStatsDto AdminUsersStats { get; set; } = new AdminUsersStatsDto();
    }
}
