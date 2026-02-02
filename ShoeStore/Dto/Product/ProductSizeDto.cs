namespace ShoeStore.Dto.Product
{
    public class ProductSizeDto
    {
        public int Id { get; set; }
        public decimal Size { get; set; }
        public int Stock { get; set; }
        public string? Sku { get; set; }
        public string Barcode { get; set; } = null!;
    }
}
