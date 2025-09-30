﻿namespace ShoeStore.Dto.Payment
{
    public class PaymentDto
    {
        public int OrderId { get; set; }

        public decimal Amount { get; set; }

        public string? Currency { get; set; }

        public string? CardBrand { get; set; }

        public string? CardLast4 { get; set; }

        public string? BillingName { get; set; }

        public string? BillingEmail { get; set; }

        public string Status { get; set; } = null!;

        public string? PaymentMethod { get; set; }
    }
}
