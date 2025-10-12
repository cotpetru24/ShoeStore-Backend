namespace ShoeStore.Dto.Admin
{
    public class RecentActivityDto
    {
        public string Source { get; set; } = string.Empty;
        public string? UserGuid { get; set; }
        public string? UserEmail { get; set; }
        public int? Id { get; set; }
        public string Description { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

}
