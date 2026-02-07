namespace ShoeStore.Dto.Admin
{
    public class AdminCreateProductFeatureRequestDto
    {
        public string FeatureText { get; set; } = null!;
        public int SortOrder { get; set; }
    }
}
