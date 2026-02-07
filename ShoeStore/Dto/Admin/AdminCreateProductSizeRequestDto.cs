namespace ShoeStore.Dto.Admin
{
    public class AdminCreateProductSizeRequestDto
    {
        public decimal Size { get; set; }
        public int Stock { get; set; }
        public string Barcode { get; set; } = null!;
    }
}
