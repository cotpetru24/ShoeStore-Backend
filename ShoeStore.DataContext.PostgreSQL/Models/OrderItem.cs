namespace ShoeStore.DataContext.PostgreSQL.Models;

public partial class OrderItem
{
    public int Id { get; set; }
    public int? OrderId { get; set; }
    public string ProductName { get; set; } = null!;
    public decimal ProductPrice { get; set; }
    public int Quantity { get; set; }
    public DateTime? CreatedAt { get; set; }
    public int ProductSizeId { get; set; }

    public virtual Order? Order { get; set; }

    public virtual ProductSize ProductSize { get; set; } = null!;
}
