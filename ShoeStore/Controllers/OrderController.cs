using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShoeStore.Dto.Order;
using ShoeStore.Services;
using System.Security.Claims;

namespace ShoeStore.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class OrderController : ControllerBase
    {
        private readonly OrderService _orderService;

        public OrderController(OrderService orderService)
        {
            _orderService = orderService;
        }


        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PlaceOrderResponseDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> PlaceOrder([FromBody] PlaceOrderRequestDto request)
        {
            var userId = User.FindFirst("Id")?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "User not authenticated" });

            var result = await _orderService.PlaceOrderAsync(request, userId);
            return Ok(result);
        }


        [HttpGet("{orderId}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(OrderDto))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetOrder(int orderId)
        {
            var userId = User.FindFirst("Id")?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "User not authenticated" });

            var order = await _orderService.GetOrderByIdAsync(orderId, userId);
            if (order == null)
                return NotFound(new { message = $"Order with ID {orderId} not found" });

            return Ok(order);
        }


        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GetOrdersResponseDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetOrders([FromQuery] GetOrdersRequestDto request)
        {
            if (request.Page < 1) request.Page = 1;
            if (request.PageSize < 1 || request.PageSize > 100) request.PageSize = 10;

            var userId = User.FindFirst("Id")?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "User not authenticated" });

            var orders = await _orderService.GetOrdersAsync(request, userId);
            return Ok(orders);
        }


        [HttpPut("cancel-order/{orderId}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(OrderDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CancelOrder(int orderId)
        {
            var userId = User.FindFirst("Id")?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "User not authenticated" });

            var result = await _orderService.CancelOrder(orderId, userId);
            if (result == null)
                return NotFound(new { message = "Order not found or cannot be cancelled" });

            return Ok(result);
        }


    }
}
