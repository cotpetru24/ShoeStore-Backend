namespace ShoeStore.Dto.Product
{
    public class GetSingleProductResponseDto
    {
        public ProductDto? Product { get; set; }
        public List<ProductDto> RelatedProducts { get; set; }
    }
}
