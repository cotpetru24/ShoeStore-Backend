using ShoeStore.Dto.Product;

namespace ShoeStore.Dto.Admin
{
    public class AdminCreateProductRequestDto
    {
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public decimal? OriginalPrice { get; set; }
        public int BrandId { get; set; }
        public int AudienceId { get; set; }
        public bool IsNew { get; set; }
        public bool IsActive { get; set; }
        public int DiscountPercentage { get; set; }
        public List<AdminCreateProductFeatureRequestDto> ProductFeatures { get; set; } = new List<AdminCreateProductFeatureRequestDto>();
        public List<AdminCreateProductSizeRequestDto> ProductSizes { get; set; } = new List<AdminCreateProductSizeRequestDto>();
        public List<ProductImageDto> ProductImages { get; set; } = new List<ProductImageDto>();

    }
}
