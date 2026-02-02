namespace ShoeStore.Dto.Admin
{
    public class AdminProductListDto
    {
        public int TotalQueryCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public string[] AllBrands { get; set; } = new string[0];
        public List<AdminProductDto> Products { get; set; } = new List<AdminProductDto>();
        public AdminProductsStatsDto AdminProductsStats { get; set; } = new AdminProductsStatsDto();
    }
}
