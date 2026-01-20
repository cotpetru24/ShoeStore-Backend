using ShoeStore.Dto.Payment;

namespace ShoeStore.Dto.Order
{
    public class OrderDto
    {
        public int Id { get; set; }
        public string? UserId { get; set; }
        public int? OrderStatusId { get; set; }
        public string? OrderStatusName { get; set; }
        public decimal Subtotal { get; set; }
        public decimal ShippingCost { get; set; }
        public decimal Discount { get; set; }
        public decimal Total { get; set; }
        public AddressDto ShippingAddress { get; set; }
        public AddressDto BillingAddress { get; set; }
        public string? Notes { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public List<OrderItemDto> OrderItems { get; set; } = new List<OrderItemDto>();
        public PaymentDto? Payment { get; set; }
    }
}


