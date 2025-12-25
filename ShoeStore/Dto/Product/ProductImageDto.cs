namespace ShoeStore.Dto.Product
{
    public class ProductImageDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ImagePath { get; set; } = null!;
        public bool IsPrimary { get; set; }
        public int SortOrder { get; set; }
    }
}
