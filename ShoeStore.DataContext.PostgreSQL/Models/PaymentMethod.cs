namespace ShoeStore.DataContext.PostgreSQL.Models;

public partial class PaymentMethod
{
    public int Id { get; set; }
    public string Code { get; set; } = null!;
    public string DisplayName { get; set; } = null!;
    public string? Description { get; set; }
    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
}
