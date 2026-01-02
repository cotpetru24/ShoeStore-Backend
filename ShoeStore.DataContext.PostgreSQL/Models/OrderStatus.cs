namespace ShoeStore.DataContext.PostgreSQL.Models;

public partial class OrderStatus
{
    public int Id { get; set; }
    public string Code { get; set; } = null!;
    public string DisplayName { get; set; } = null!;
    public string? Description { get; set; }
    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}
