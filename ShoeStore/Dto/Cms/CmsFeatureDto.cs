namespace ShoeStore.Dto.Cms
{
    public class CmsFeatureDto
    {

        public int Id { get; set; }
        public string IconClass { get; set; } = null!;
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public int SortOrder { get; set; }
    }
}
