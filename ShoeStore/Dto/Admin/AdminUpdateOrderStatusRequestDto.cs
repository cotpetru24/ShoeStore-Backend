using ShoeStore.Dto.Order;

namespace ShoeStore.Dto.Admin
{
    public class AdminUpdateOrderStatusRequestDto
    {
        public OrderStatusEnum StatusId { get; set; }
        public string? Notes { get; set; }
    }
}
