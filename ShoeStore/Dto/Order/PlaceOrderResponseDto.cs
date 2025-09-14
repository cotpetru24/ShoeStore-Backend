namespace ShoeStore.Dto.Order
{
    public class PlaceOrderResponseDto
    {
        public int OrderId { get; set; }
        public string Message { get; set; } = string.Empty;
        public decimal Total { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
