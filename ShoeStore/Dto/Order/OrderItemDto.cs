namespace ShoeStore.Dto.Order
{
    public class OrderItemDto
    {
        public int Id { get; set; }
        public int? OrderId { get; set; }
        public int? ProductId { get; set; }
        public string ProductName { get; set; } = null!;
        public decimal ProductPrice { get; set; }
        public int Quantity { get; set; }
        public string? Size { get; set; }
        public string Barcode { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string? MainImage { get; set; }
        public string? BrandName {get; set; }
    }
}


