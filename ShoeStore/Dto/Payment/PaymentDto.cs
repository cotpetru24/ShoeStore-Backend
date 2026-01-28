using System.ComponentModel.DataAnnotations;

namespace ShoeStore.Dto.Payment
{
    public class PaymentDto
    {
        public int OrderId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public string? CardBrand { get; set; }
        public string? CardLast4 { get; set; }
        public string Status { get; set; } = null!;
        public string PaymentMethod { get; set; }
        public string ReceiptUrl { get; set; }
    }


    public class CreatePaymentIntentRequestDto
    {
        [Range(1, long.MaxValue)]
        public required long Amount { get; set; }
    }


    public class CreatePaymentIntentResponseDto
    {
        public string ClientSecret { get; set; } = string.Empty;
    }
}

