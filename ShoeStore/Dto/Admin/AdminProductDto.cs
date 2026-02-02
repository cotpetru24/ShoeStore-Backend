using ShoeStore.Dto.Product;

namespace ShoeStore.Dto.Admin
{
    public class AdminProductDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public decimal? OriginalPrice { get; set; }
        public int TotalStock => ProductSizes?.Sum(s => s.Stock) ?? 0;
        public int BrandId { get; set; }
        public required string BrandName { get; set; }
        public int AudienceId { get; set; }
        public required string Audience { get; set; }
        public decimal Rating { get; set; }
        public int ReviewCount { get; set; }
        public bool IsNew { get; set; }
        public bool IsActive { get; set; }
        public int DiscountPercentage { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public List<ProductFeatureDto> ProductFeatures { get; set; } = new List<ProductFeatureDto>();
        public List<ProductSizeDto> ProductSizes { get; set; } = new List<ProductSizeDto>();
        public List<ProductImageDto> ProductImages { get; set; } = new List<ProductImageDto>();
    }
}
