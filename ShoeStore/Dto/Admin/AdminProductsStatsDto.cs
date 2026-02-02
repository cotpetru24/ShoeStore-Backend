namespace ShoeStore.Dto.Admin
{
    public class AdminProductsStatsDto
    {
        public int TotalProductsCount { get; set; }
        public int TotalLowStockProductsCount { get; set; }
        public int TotalOutOfStockProductsCount { get; set; }
        public int TotalActiveProductsCount { get; set; }
    }
}
