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

        public decimal ShippingCost { get; set; } = 0;

        public decimal Discount { get; set; } = 0;

        public string? Notes { get; set; }
    }

    public class OrderItemRequestDto
    {
        public required int ProductId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
        public required int Quantity { get; set; }

        public required string ProductSizeBarcode { get; set; }
    }
}


