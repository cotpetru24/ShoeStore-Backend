using System;
using System.Collections.Generic;

namespace ShoeStore.Dto.Admin
{
    public class AdminProductDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public decimal? OriginalPrice { get; set; }
        public string? ImagePath { get; set; }
        public int Stock { get; set; }
        public int? BrandId { get; set; }
        public string? BrandName { get; set; }
        public int? AudienceId { get; set; }
        public string? AudienceName { get; set; }
        public decimal? Rating { get; set; }
        public int? ReviewCount { get; set; }
        public bool? IsNew { get; set; }
        public int? DiscountPercentage { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public List<AdminProductSizeDto> ProductSizes { get; set; } = new List<AdminProductSizeDto>();
        public List<AdminProductImageDto> ProductImages { get; set; } = new List<AdminProductImageDto>();
    }

    public class AdminProductListDto
    {
        public List<AdminProductDto> Products { get; set; } = new List<AdminProductDto>();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }

    public class AdminProductSizeDto
    {
        public int Id { get; set; }
        public string Size { get; set; } = null!;
        public int Stock { get; set; }
    }

    public class AdminProductImageDto
    {
        public int Id { get; set; }
        public string ImagePath { get; set; } = null!;
        public bool IsPrimary { get; set; }
        public int SortOrder { get; set; }
    }

    public class CreateProductRequestDto
    {
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public decimal? OriginalPrice { get; set; }
        public string? ImagePath { get; set; }
        public int Stock { get; set; }
        public int? BrandId { get; set; }
        public int? AudienceId { get; set; }
        public bool? IsNew { get; set; }
        public int? DiscountPercentage { get; set; }
        public List<CreateProductSizeRequestDto> ProductSizes { get; set; } = new List<CreateProductSizeRequestDto>();
        public List<CreateProductImageRequestDto> ProductImages { get; set; } = new List<CreateProductImageRequestDto>();
    }

    public class UpdateProductRequestDto
    {
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public decimal? OriginalPrice { get; set; }
        public string? ImagePath { get; set; }
        public int Stock { get; set; }
        public int? BrandId { get; set; }
        public int? AudienceId { get; set; }
        public bool? IsNew { get; set; }
        public int? DiscountPercentage { get; set; }
    }

    public class CreateProductSizeRequestDto
    {
        public string Size { get; set; } = null!;
        public int Stock { get; set; }
    }

    public class CreateProductImageRequestDto
    {
        public string ImagePath { get; set; } = null!;
        public bool IsPrimary { get; set; }
        public int SortOrder { get; set; }
    }

    public class GetProductsRequestDto
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SearchTerm { get; set; }
        public int? BrandId { get; set; }
        public int? AudienceId { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public bool? IsNew { get; set; }
        public bool? LowStock { get; set; }
        public string? SortBy { get; set; } = "CreatedAt";
        public string? SortDirection { get; set; } = "desc";
    }
}
