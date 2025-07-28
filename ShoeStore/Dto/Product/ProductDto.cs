using ShoeStore.DataContext.PostgreSQL.Models;

namespace ShoeStore.Dto.Product
{
    public class ProductDto
    {

        public int Id { get; set; }

        public string Name { get; set; } = null!;

        public string? Description { get; set; } = null;

        public decimal Price { get; set; }

        public decimal? OriginalPrice { get; set; } = null;

        public string? ImagePath { get; set; } = null;

        public int Stock { get; set; }

        public string BrandName { get; set; } = string.Empty;

        public string Audience { get; set; } = string.Empty;

        public decimal? Rating { get; set; } = null;

        public int? ReviewCount { get; set; } = null;

        public bool? IsNew { get; set; } = null;

        public int? DiscountPercentage { get; set; } = null;

    }
}
