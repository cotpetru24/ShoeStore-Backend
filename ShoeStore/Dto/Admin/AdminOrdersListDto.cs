namespace ShoeStore.Dto.Admin
{
    public class AdminOrdersListDto
    {
        public List<AdminOrderDto> Orders { get; set; } = new List<AdminOrderDto>();
        public int TotalQueryCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public AdminOrdersStatsDto AdminOrdersStats { get; set; } = new AdminOrdersStatsDto();
    }
}
