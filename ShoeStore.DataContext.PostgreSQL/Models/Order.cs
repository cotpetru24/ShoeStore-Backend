using System;
using System.Collections.Generic;

namespace ShoeStore.DataContext.PostgreSQL.Models;

public partial class Order
{
    public int Id { get; set; }

    public string UserId { get; set; } = null!;

    public int OrderStatus { get; set; }

    public decimal Subtotal { get; set; }

    public decimal ShippingCost { get; set; }

    public decimal Discount { get; set; }

    public decimal Total { get; set; }

    public int ShippingAddressId { get; set; }

    public int BillingAddressId { get; set; }

    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public int? PaymentId { get; set; }

    public virtual UserAddress BillingAddress { get; set; } = null!;

    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    public virtual Payment? Payment { get; set; }

    public virtual UserAddress ShippingAddress { get; set; } = null!;

    public virtual AspNetUser User { get; set; } = null!;
}
