namespace ShoeStore.Dto.Product
{
    public class GetProductsResposeDto
    {
        public List<ProductDto> Products { get; set; }
        public string [] Brands {get;set;}

    }
}
