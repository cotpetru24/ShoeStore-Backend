namespace ShoeStore.Dto.Admin
{
    public class AdminUsersStatsDto
    {
        public int TotalUsersCount { get; set; }
        public int TotalActiveUsersCount { get; set; }
        public int TotalBlockedUsersCount { get; set; }
        public int TotalNewUsersCountThisMonth { get; set; }
    }
}
