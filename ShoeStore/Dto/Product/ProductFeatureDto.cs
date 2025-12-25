namespace ShoeStore.Dto.Product
{
    public class ProductFeatureDto
    {
        public int Id { get; set; }
        public string FeatureText { get; set; } = null!;
        public int SortOrder { get; set; }
    }
}
