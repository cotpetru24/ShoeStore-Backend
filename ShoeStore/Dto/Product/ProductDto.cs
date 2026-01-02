namespace ShoeStore.Dto.Product
{
    public class ProductDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; } = null;
        public decimal Price { get; set; }
        public decimal? OriginalPrice { get; set; } = null;
        public int TotalStock { get; set; }
        public string BrandName { get; set; } = string.Empty;
        public string Audience { get; set; } = string.Empty;
        public decimal? Rating { get; set; } = null;
        public int? ReviewCount { get; set; } = null;
        public bool? IsNew { get; set; } = null;
        public bool IsActive { get; set; } = true;
        public decimal? DiscountPercentage { get; set; } = null;
        public List<ProductImageDto> ProductImages { get; set; } = new List<ProductImageDto>();
        public List<ProductSizeDto> ProductSizes { get; set; } = new List<ProductSizeDto>();
        public List<ProductFeatureDto> ProductFeatures { get; set; } = new List<ProductFeatureDto>();
    }
}
