namespace ShoeStore.Dto.Admin
{
    public class GetAdminProductsRequestDto
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SearchTerm { get; set; }
        public string? ProductBrand { get; set; }
        public int? AudienceId { get; set; }
        public bool? IsActive { get; set; }
        public AdminProductStockStatusEnum? ProductStockStatus { get; set; }
        public AdminProductsSortByEnum? SortBy { get; set; }
        public SortDirectionEnum? SortDirection { get; set; }
    }
}
