using ShoeStore.Dto.Order;

namespace ShoeStore.Dto.Admin
{
    public class AdminUpdateOrderStatusRequestDto
    {
        public OrderStatusEnum Status { get; set; }
        public string? Notes { get; set; }
    }
}
