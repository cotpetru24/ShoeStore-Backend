namespace ShoeStore.DataContext.PostgreSQL.Models;

public partial class ProductReview
{
    public int Id { get; set; }
    public int? ProductId { get; set; }
    public string? UserId { get; set; }
    public int Rating { get; set; }
    public string? Comment { get; set; }
    public DateTime? CreatedAt { get; set; }

    public virtual Product? Product { get; set; }
}
