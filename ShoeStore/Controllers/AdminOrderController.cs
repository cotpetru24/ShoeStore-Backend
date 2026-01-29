using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShoeStore.Dto.Admin;
using ShoeStore.Services;

namespace ShoeStore.Controllers
{
    [ApiController]
    [Route("api/admin/orders")]
    [Authorize(Roles = "Administrator")]
    public class AdminOrderController : ControllerBase
    {
        private readonly AdminOrderService _adminOrderService;

        public AdminOrderController(AdminOrderService adminOrderService)
        {
            _adminOrderService = adminOrderService;
        }


        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AdminOrderListDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<AdminOrderListDto>> GetOrdersAsync([FromQuery] GetAdminOrdersRequestDto request)
        {
            if (request.PageNumber < 1) request.PageNumber = 1;
            if (request.PageSize < 1 || request.PageSize > 100) request.PageSize = 10;

            var orders = await _adminOrderService.GetOrdersAsync(request);
            return Ok(orders);
        }


        [HttpGet("{orderId}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AdminOrderDto))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<AdminOrderDto>> GetOrderByIdAsync(int orderId)
        {
            var order = await _adminOrderService.GetOrderByIdAsync(orderId);
            if (order == null)
                return NotFound(new { message = "Order not found" });

            return Ok(order);
        }


        [HttpPut("{orderId}/status")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateOrderStatusAsync(int orderId, [FromBody] UpdateOrderStatusRequestDto request)
        {
            var success = await _adminOrderService.UpdateOrderStatusAsync(orderId, request);
            if (!success)
            {
                return NotFound(new { message = "Order not found or update failed" });
            }

            return Ok(new { message = "Order status updated successfully" });
        }
    }
}
