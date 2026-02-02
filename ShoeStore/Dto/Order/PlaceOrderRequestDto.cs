using ShoeStore.Dto.Address;
using System.ComponentModel.DataAnnotations;

namespace ShoeStore.Dto.Order
{
    public class PlaceOrderRequestDto
    {
        [Required]
        public List<OrderItemRequestDto> OrderItems { get; set; } = new List<OrderItemRequestDto>();

        [Required]
        public int ShippingAddressId { get; set; }

        public int? BillingAddressId { get; set; }

        public bool BillingAddressSameAsShipping { get; set; }

        public CreateAddressRequestDto? BillingAddressRequest { get; set; }

        public decimal ShippingCost { get; set; }

        public decimal Discount { get; set; } = 0;

        public string? Notes { get; set; }

        public string PaymentIntentId { get; set; } = null;
    }

}


