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
        [ProducesResponseType(200, Type = typeof(PlaceOrderResponseDto))]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> PlaceOrder([FromBody] PlaceOrderRequestDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var userId = User.FindFirst("Id")?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authenticated");

                var result = await _orderService.PlaceOrderAsync(request, userId);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch
            {
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("{orderId}")]
        [ProducesResponseType(200, Type = typeof(OrderDto))]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetOrder(int orderId)
        {
            try
            {
                var userId = User.FindFirst("Id")?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authenticated");

                var order = await _orderService.GetOrderByIdAsync(orderId, userId);
                if (order == null)
                    return NotFound($"Order with ID {orderId} not found");

                return Ok(order);
            }
            catch
            {
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet]
        [ProducesResponseType(200, Type = typeof(GetOrdersResponseDto))]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetOrders([FromQuery] GetOrdersRequestDto request)
        {
            try
            {
                var userId = User.FindFirst("Id")?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authenticated");

                var orders = await _orderService.GetOrdersAsync(request, userId);
                return Ok(orders);
            }
            catch
            {
                return StatusCode(500, "Internal server error");
            }
        }

        // Shipping Address Endpoints
        [HttpPost("shipping-addresses")]
        [ProducesResponseType(200, Type = typeof(ShippingAddressResponseDto))]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> CreateShippingAddress([FromBody] CreateShippingAddressRequestDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var userId = User.FindFirst("Id")?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authenticated");

                var result = await _orderService.CreateShippingAddressAsync(request, userId);
                var result2 = await _orderService.CreateBillingAddressAsync(request, userId);

                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch
            {
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("shipping-addresses")]
        [ProducesResponseType(200, Type = typeof(List<ShippingAddressDto>))]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetShippingAddresses()
        {
            try
            {
                var userId = User.FindFirst("Id")?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authenticated");

                var addresses = await _orderService.GetShippingAddressesAsync(userId);
                return Ok(addresses);
            }
            catch
            {
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("shipping-addresses/{addressId}")]
        [ProducesResponseType(200, Type = typeof(ShippingAddressDto))]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetShippingAddress(int addressId)
        {
            try
            {
                var userId = User.FindFirst("Id")?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authenticated");

                var address = await _orderService.GetShippingAddressByIdAsync(addressId, userId);
                if (address == null)
                    return NotFound($"Shipping address with ID {addressId} not found");

                return Ok(address);
            }
            catch
            {
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPut("shipping-addresses/{addressId}")]
        [ProducesResponseType(200, Type = typeof(ShippingAddressResponseDto))]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> UpdateShippingAddress(int addressId, [FromBody] UpdateShippingAddressRequestDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var userId = User.FindFirst("Id")?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authenticated");

                var result = await _orderService.UpdateShippingAddressAsync(addressId, request, userId);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch
            {
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpDelete("shipping-addresses/{addressId}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> DeleteShippingAddress(int addressId)
        {
            try
            {
                var userId = User.FindFirst("Id")?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authenticated");

                var result = await _orderService.DeleteShippingAddressAsync(addressId, userId);
                if (!result)
                    return NotFound($"Shipping address with ID {addressId} not found");

                return Ok(new { message = "Shipping address deleted successfully" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch
            {
                return StatusCode(500, "Internal server error");
            }
        }

        // Billing Address Endpoints
        [HttpPost("billing-addresses")]
        [Authorize]
        [ProducesResponseType(200, Type = typeof(BillingAddressResponseDto))]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> CreateBillingAddress([FromBody] CreateBillingAddressRequestDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var userId = User.FindFirst("Id")?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authenticated");

                //var result = await _orderService.CreateBillingAddressAsync(request, userId);
                return Ok();
                //return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch
            {
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("billing-addresses")]
        [Authorize]
        [ProducesResponseType(200, Type = typeof(List<BillingAddressDto>))]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetBillingAddresses()
        {
            try
            {
                var userId = User.FindFirst("Id")?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authenticated");
                var id = 123; // Test 
                var addresses = await _orderService.GetBillingAddressesAsync(id);
                return Ok(addresses);
            }
            catch
            {
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("billing-addresses/{addressId}")]
        [Authorize]
        [ProducesResponseType(200, Type = typeof(BillingAddressDto))]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetBillingAddress(int addressId)
        {
            try
            {
                var userId = User.FindFirst("Id")?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authenticated");

                var address = await _orderService.GetBillingAddressByIdAsync(addressId, userId);
                if (address == null)
                    return NotFound("Billing address not found");

                return Ok(address);
            }
            catch
            {
                return StatusCode(500, "Internal server error");
            }
        }


        [HttpPut("cancel-order/{orderId}")]
        [ProducesResponseType(200, Type = typeof(OrderDto))]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> CancelOrder(int orderId)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var userId = User.FindFirst("Id")?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authenticated");

                var result = await _orderService.CancelOrder(orderId, userId);

                if (result == null) return NotFound("Order not found or cannot be cancelled");

                return Ok(result);
                                   }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch
            {
                return StatusCode(500, "Internal server error");
            }
        }



    }
}
