namespace ShoeStore.Dto.Cms
{
    public class CmsCategoryDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string? ImageBase64 { get; set; }
        public string? ItemTagline { get; set; }
        public int SortOrder { get; set; }
    }

}
