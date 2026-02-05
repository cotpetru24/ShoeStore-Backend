using ShoeStore.Dto.Address;
using ShoeStore.Dto.Order;

namespace ShoeStore.Dto.Admin
{
    public class AdminOrderDto
    {
        public int Id { get; set; }
        public string UserId { get; set; } = null!;
        public string UserEmail { get; set; } = null!;
        public string UserName { get; set; } = null!;
        public OrderStatusEnum Status { get; set; }
        public decimal Subtotal { get; set; }
        public decimal ShippingCost { get; set; }
        public decimal Discount { get; set; }
        public decimal Total { get; set; }
        public string? Notes { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public AddressDto? ShippingAddress { get; set; }
        public AddressDto? BillingAddress { get; set; }
        public List<OrderItemDto> OrderItems { get; set; } = new List<OrderItemDto>();
        public AdminPaymentDto Payment { get; set; } = new AdminPaymentDto();
    }
}
