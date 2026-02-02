namespace ShoeStore.Dto.Product
{
    public class GetProductsResponseDto
    {
        public List<ProductDto> Products { get; set; } = new List<ProductDto>();
        public string[] Brands { get; set; } = new string[0];
    }
}
