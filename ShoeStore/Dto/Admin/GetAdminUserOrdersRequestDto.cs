namespace ShoeStore.Dto.Admin
{
    public class GetAdminUserOrdersRequestDto
    {
        public string UserId { get; set; } = null!;
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public OrderStatusEnum? StatusFilter { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }
}
