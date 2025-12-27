namespace ShoeStore.Dto.Cms
{
    public class CmsStoredProfileDto
    {
        public int Id { get; set; }
        public string ProfileName { get; set; } = null!;
        public bool IsActive { get; set; }
        public DateTime LastModified { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
