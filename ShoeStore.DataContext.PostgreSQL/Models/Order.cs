using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;

namespace ShoeStore.DataContext.PostgreSQL.Models;

public partial class Order
{
    public int Id { get; set; }

    public string? UserId { get; set; }

    public int? OrderStatusId { get; set; }

    public decimal Subtotal { get; set; }

    public decimal ShippingCost { get; set; }

    public decimal Discount { get; set; }

    public decimal Total { get; set; }

    public int? ShippingAddressId { get; set; }

    public int? BillingAddressId { get; set; }

    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual BillingAddress? BillingAddress { get; set; }

    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    public virtual OrderStatus? OrderStatus { get; set; }

    public virtual UserDetail? UserDetail { get; set; }

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    public virtual ShippingAddress? ShippingAddress { get; set; }


}
