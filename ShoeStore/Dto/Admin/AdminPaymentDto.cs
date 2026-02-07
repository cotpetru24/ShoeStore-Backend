namespace ShoeStore.Dto.Admin
{
    public class AdminPaymentDto
    {
        public int Id { get; set; }
        public PaymentStatusEnum? Status { get; set; }
        public string? TransactionId { get; set; }
        public DateTime? CreatedAt { get; set; }
        public decimal Amount { get; set; }
        public string? Currency { get; set; }
        public string? CardBrand { get; set; }
        public string? CardLast4 { get; set; }
        public string? PaymentMethod { get; set; }
        public string? ReceiptUrl { get; set; }
    }
}
