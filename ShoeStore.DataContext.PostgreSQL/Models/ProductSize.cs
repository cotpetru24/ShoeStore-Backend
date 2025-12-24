using System;
using System.Collections.Generic;

namespace ShoeStore.DataContext.PostgreSQL.Models;

public partial class ProductSize
{
    public int Id { get; set; }

    public int ProductId { get; set; }

    public decimal UkSize { get; set; }

    public int Stock { get; set; }

    public string Barcode { get; set; } = null!;

    public string Sku { get; set; } = null!;

    public virtual Product Product { get; set; } = null!;
}
