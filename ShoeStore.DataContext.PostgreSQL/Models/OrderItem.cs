using System;
using System.Collections.Generic;

namespace ShoeStore.DataContext.PostgreSQL.Models;

public partial class OrderItem
{
    public int Id { get; set; }

    public int OrderId { get; set; }

    public decimal UnitPrice { get; set; }

    public int Quantity { get; set; }

    public DateTime CreatedAt { get; set; }

    public int ProductSizeId { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual Order Order { get; set; } = null!;

    public virtual ProductSize ProductSize { get; set; } = null!;
}
