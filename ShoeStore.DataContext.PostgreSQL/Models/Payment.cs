using System;
using System.Collections.Generic;

namespace ShoeStore.DataContext.PostgreSQL.Models;

public partial class Payment
{
    public int Id { get; set; }

    public int PaymentMethodId { get; set; }

    public int PaymentStatus { get; set; }

    public decimal Amount { get; set; }

    public string Currency { get; set; } = null!;

    public string? TransactionId { get; set; }

    public DateTime? ProcessedAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public string PaymentIntentId { get; set; } = null!;

    public string? CardBrand { get; set; }

    public string? CardLast4 { get; set; }

    public string? ReceiptUrl { get; set; }

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public virtual PaymentMethod PaymentMethod { get; set; } = null!;
}
