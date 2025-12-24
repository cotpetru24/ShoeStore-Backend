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
        public int TotalStock => ProductSizes?.Sum(s => s.Stock) ?? 0;
        public int? BrandId { get; set; }
        public string? BrandName { get; set; }
        public int? AudienceId { get; set; }
        public string? Audience { get; set; }
        public decimal? Rating { get; set; }
        public int? ReviewCount { get; set; }
        public bool? IsNew { get; set; }
        public bool IsActive { get; set; }
        public int? DiscountPercentage { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public List<AdminProductFeatureDto> ProductFeatures { get; set; } = new List<AdminProductFeatureDto>();
        public List<AdminProductSizeDto> ProductSizes { get; set; } = new List<AdminProductSizeDto>();
        public List<AdminProductImageDto> ProductImages { get; set; } = new List<AdminProductImageDto>();
    }

    public class AdminProductListDto
    {
        public List<AdminProductDto> Products { get; set; } = new List<AdminProductDto>();
        public int TotalQueryCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public AdminProductsStatsDto AdminProductsStats { get; set; }
        public string[] AllBrands { get; set; }
    }

    public class AdminProductFeatureDto
    {
        public int Id { get; set; }
        public string FeatureText { get; set; } = null!;
        public int SortOrder { get; set; }
    }


    public class AdminProductSizeDto
    {
        public int Id { get; set; }
        public decimal Size { get; set; }
        public int Stock { get; set; }
        public string? Sku { get; set; }
        public string Barcode { get; set; }
    }

    public class AdminProductImageDto
    {
        public int Id { get; set; }
        public string ImagePath { get; set; } = null!;
        public bool IsPrimary { get; set; }
        public int SortOrder { get; set; }
    }


    public class AdminBrandDto
    {
        public int BrandId { get; set; }
        public string BrandName { get; set; }
    }

    public class AdminAudienceDto
    {
        public int AudienceId { get; set; }
        public string AudienceName { get; set; }
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
        public string? ProductBrand { get; set; }
        public string? ProductCategory { get; set; }
        public bool? IsActive { get; set; }
        public string? ProductStockStatus {get; set;}
        public string? SortBy { get; set; }
        public string? SortDirection { get; set; }


        //public decimal? MinPrice { get; set; }
        //public decimal? MaxPrice { get; set; }
        //public bool? IsNew { get; set; }
        //public bool? LowStock { get; set; }

    }



    public class AdminProductsStatsDto
    {
        public int TotalProductsCount { get; set; }
        public int TotalLowStockProductsCount { get; set; }
        public int TotalOutOfStockProductsCount { get; set; }
        public int TotalActiveProductsCount { get; set; }
    }




    public enum AdminProductStockStatus
    {
        LowStock = 1,
        HighStock = 2,
        InStock = 3,
        OutOfStock = 4
    }

    public enum AdminProductStatus
    {
        Active = 1,
        Inactive = 2,
    }

    public enum AdminProductsSortBy
    {
        DateCreated = 1,
        Name = 2,
        Stock = 3
    }

    public enum AdminProductsSortDirection
    {
        Ascending = 1,
        Descending = 2,
    }

    public static class AdminProductEnumMappings
    {
        public static AdminProductStatus? MapAdminProductStatus(string? value) =>
            value?.ToLower() switch
            {
                "active" => AdminProductStatus.Active,
                "inactive" => AdminProductStatus.Inactive,
                _ => null
            };

        public static AdminProductStockStatus? MapAdminProductStockStatus(string? value) =>
            value?.ToLower() switch
            {
                "low stock" => AdminProductStockStatus.LowStock,
                "high stock" => AdminProductStockStatus.HighStock,
                "in stock" => AdminProductStockStatus.InStock,
                "out of stock" => AdminProductStockStatus.OutOfStock,
                _ => null
            };

        public static AdminProductsSortBy? MapAdminProductsSortBy(string? value) =>
            value?.ToLower() switch
            {
                "createdat" => AdminProductsSortBy.DateCreated,
                "name" => AdminProductsSortBy.Name,
                "stock" => AdminProductsSortBy.Stock,
                _ => null
            };

        public static AdminProductsSortDirection? MapAdminProductsSortDirection(string? value) =>
            value?.ToLower() switch
            {
                "asc" => AdminProductsSortDirection.Ascending,
                "desc" => AdminProductsSortDirection.Descending,
                _ => null
            };
    }

}
