namespace ShoeStore.DataContext.PostgreSQL.Models;

public partial class Payment
{
    public int Id { get; set; }
    public int? OrderId { get; set; }
    public int? PaymentMethodId { get; set; }
    public int? PaymentStatusId { get; set; }
    public decimal Amount { get; set; }
    public string? Currency { get; set; }
    public string? TransactionId { get; set; }
    public string? GatewayResponse { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? PaymentIntentId { get; set; }
    public string? CustomerId { get; set; }
    public string? CardBrand { get; set; }
    public string? CardLast4 { get; set; }
    public string? BillingName { get; set; }
    public string? BillingEmail { get; set; }
    public string? ReceiptUrl { get; set; }

    public virtual Order? Order { get; set; }

    public virtual PaymentMethod? PaymentMethod { get; set; }

    public virtual PaymentStatus PaymentStatus { get; set; }
}
