using System.ComponentModel.DataAnnotations;

namespace ShoeStore.Dto.Order
{
    public class OrderItemRequestDto
    {
        public required int ProductId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
        public required int Quantity { get; set; }

        public required string ProductSizeBarcode { get; set; }
    }
}
