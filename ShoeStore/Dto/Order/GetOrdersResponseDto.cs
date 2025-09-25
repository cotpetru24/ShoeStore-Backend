namespace ShoeStore.Dto.Order
{
    public class GetOrdersResponseDto
    {
        public List<OrderDto> Orders { get; set; } = new List<OrderDto>();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }
}


