namespace ShoeStore.Dto.Admin
{
    public class GetAdminOrdersRequestDto
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SearchTerm { get; set; }
        public OrderStatusEnum? StatusFilter { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public OrdersSortByEnum SortBy { get; set; } = OrdersSortByEnum.Date;
        public SortDirectionEnum SortDirection { get; set; } = SortDirectionEnum.Descending;
    }
}
