namespace ShoeStore.Dto.Product
{
    public class GetProductsRequest
    {
        public string? Category { get; set; } = null;
        public string? Brand { get; set; } = null;
        public decimal? MinPrice { get; set; } = null;
        public decimal? MaxPrice { get; set; } = null;
        public string? Search { get; set; } = null;
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 30;
    }

}
